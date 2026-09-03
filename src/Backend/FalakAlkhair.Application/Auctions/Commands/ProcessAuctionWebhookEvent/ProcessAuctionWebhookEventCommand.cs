using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.ProcessAuctionWebhookEvent;

/// <summary>
/// معالجة حدث وارد من منصة المزادات المستقلة عبر Webhook. يُسجَّل الحدث دائمًا
/// في سجل التدقيق (Append-Only، غير قابل للتعديل لاحقًا) بغضّ النظر عن نوعه،
/// وتُطبَّق فقط تحديثات معلوماتية آمنة على المزاد (لا قرارات مالية تلقائية —
/// الإرساء والتسوية يبقيان أمرين داخليين صريحين لضمان سلامة العمليات المالية).
/// </summary>
public record ProcessAuctionWebhookEventCommand : IRequest
{
    public string ExternalAuctionId { get; init; } = default!;
    public string EventType { get; init; } = default!;
    public decimal? BidAmount { get; init; }
    public DateTime? NewEndDate { get; init; }
    public DateTime? OccurredAt { get; init; }
    public string? RawPayload { get; init; }
    public string? SourceIp { get; init; }
}

public class ProcessAuctionWebhookEventCommandValidator : AbstractValidator<ProcessAuctionWebhookEventCommand>
{
    public ProcessAuctionWebhookEventCommandValidator()
    {
        RuleFor(x => x.ExternalAuctionId).NotEmpty();
        RuleFor(x => x.EventType).NotEmpty()
            .Must(v => Enum.TryParse<AuctionEventType>(v, out _))
            .WithMessage("نوع الحدث غير معروف.");
    }
}

public class ProcessAuctionWebhookEventCommandHandler : IRequestHandler<ProcessAuctionWebhookEventCommand>
{
    private readonly IApplicationDbContext _context;

    public ProcessAuctionWebhookEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProcessAuctionWebhookEventCommand request, CancellationToken cancellationToken)
    {
        // بحث غير مقيَّد بالشركة عمدًا: هذا نداء وارد من نظام خارجي (Webhook) وليس
        // مستخدمًا مصادَقًا عليه ينتمي لشركة محدَّدة — المعرّف الخارجي فريد عالميًا.
        var auction = await _context.Auctions
            .FirstOrDefaultAsync(a => a.ExternalAuctionId == request.ExternalAuctionId && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.ExternalAuctionId);

        var eventType = Enum.Parse<AuctionEventType>(request.EventType);

        switch (eventType)
        {
            case AuctionEventType.BidPlaced:
                if (request.BidAmount.HasValue) auction.CurrentBidAmount = request.BidAmount.Value;
                auction.BidsCount += 1;
                break;

            case AuctionEventType.AuctionExtended:
                if (request.NewEndDate.HasValue) auction.EndDate = request.NewEndDate.Value;
                break;

            case AuctionEventType.AuctionWentLive:
                if (auction.Status == AuctionStatus.Published) auction.Status = AuctionStatus.Live;
                break;

            case AuctionEventType.AuctionEnded:
                if (auction.Status == AuctionStatus.Live) auction.Status = AuctionStatus.Ended;
                break;
        }

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = eventType,
            Payload = request.RawPayload,
            SourceIp = request.SourceIp,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            Notes = "حدث وارد من منصة المزادات المستقلة عبر Webhook."
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
