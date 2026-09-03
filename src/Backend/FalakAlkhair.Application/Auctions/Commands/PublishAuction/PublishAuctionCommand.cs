using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.PublishAuction;

/// <summary>
/// نشر المزاد: Scheduled → Published. تُنفَّذ حالة النشر الداخلية دائمًا بنجاح؛
/// محاولة مزامنته مع منصة المزادات الخارجية (IAuctionPlatformClient) تحدث
/// بأفضل جهد (Best-Effort) — إن لم تكن المنصة مُكوَّنة بعد، يُسجَّل ذلك في سجل
/// التدقيق دون فشل العملية الداخلية (لا يُفترض توفر التكامل الخارجي، البند 32).
/// </summary>
public record PublishAuctionCommand(Guid Id) : IRequest;

public class PublishAuctionCommandHandler : IRequestHandler<PublishAuctionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuctionPlatformClient _auctionPlatformClient;

    public PublishAuctionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IAuctionPlatformClient auctionPlatformClient)
    {
        _context = context;
        _currentUser = currentUser;
        _auctionPlatformClient = auctionPlatformClient;
    }

    public async Task Handle(PublishAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        if (auction.Status != AuctionStatus.Scheduled)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن نشر مزاد بحالته الحالية ({auction.Status}). يجب اعتماده أولًا.");
        }

        auction.Status = AuctionStatus.Published;

        string auditNotes;
        try
        {
            auction.ExternalAuctionId = await _auctionPlatformClient.PublishAuctionAsync(auction, cancellationToken);
            auditNotes = $"تم النشر داخليًا ومزامنته مع منصة المزادات (المعرّف الخارجي: {auction.ExternalAuctionId}).";
        }
        catch (Common.Exceptions.BusinessRuleException ex)
        {
            auditNotes = $"تم النشر داخليًا فقط — تعذّرت المزامنة مع المنصة الخارجية: {ex.Message}";
        }

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = AuctionEventType.AuctionPublished,
            OccurredAt = DateTime.UtcNow,
            Notes = auditNotes
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
