using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.UpdateAuctionStatus;

/// <summary>
/// تحديث حالة المزاد يدويًا لأحد الانتقالات الحرة: Draft→PendingApproval،
/// Published→Live، Live→Ended، أو الإلغاء من أي حالة غير نهائية. الانتقالات
/// الأخرى (Scheduled، Published، Awarded، Settled) لها أوامر مخصَّصة (اعتماد،
/// نشر، إرساء، تسوية) ولا تُضبَط هنا مباشرة.
/// </summary>
public record UpdateAuctionStatusCommand : IRequest
{
    public Guid Id { get; init; }
    public AuctionStatus Status { get; init; }
    public string? CancellationReason { get; init; }
}

public class UpdateAuctionStatusCommandValidator : AbstractValidator<UpdateAuctionStatusCommand>
{
    public UpdateAuctionStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CancellationReason).NotEmpty().When(x => x.Status == AuctionStatus.Cancelled)
            .WithMessage("سبب الإلغاء مطلوب.");
    }
}

public class UpdateAuctionStatusCommandHandler : IRequestHandler<UpdateAuctionStatusCommand>
{
    private static readonly HashSet<AuctionStatus> RestrictedTargets = new()
    {
        AuctionStatus.Scheduled, AuctionStatus.Published, AuctionStatus.Awarded, AuctionStatus.Settled
    };

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAuctionStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAuctionStatusCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        if (auction.Status is AuctionStatus.Awarded or AuctionStatus.Settled or AuctionStatus.Cancelled)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن تعديل مزاد بحالته الحالية ({auction.Status}).");
        }

        if (request.Status == AuctionStatus.Cancelled)
        {
            auction.Status = AuctionStatus.Cancelled;
            auction.CancellationReason = request.CancellationReason;

            _context.AuctionAuditLogs.Add(new AuctionAuditLog
            {
                CompanyId = auction.CompanyId,
                BranchId = auction.BranchId,
                AuctionId = auction.Id,
                EventType = AuctionEventType.AuctionCancelled,
                OccurredAt = DateTime.UtcNow,
                Notes = request.CancellationReason
            });

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (RestrictedTargets.Contains(request.Status))
        {
            throw new Common.Exceptions.BusinessRuleException(
                $"لا يمكن ضبط الحالة \"{request.Status}\" مباشرة — استخدم أمر الاعتماد/النشر/الإرساء/التسوية المخصَّص.");
        }

        if (request.Status <= auction.Status)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن الرجوع لحالة سابقة أو نفس الحالة الحالية.");
        }

        auction.Status = request.Status;

        var eventType = request.Status switch
        {
            AuctionStatus.Live => AuctionEventType.AuctionWentLive,
            AuctionStatus.Ended => AuctionEventType.AuctionEnded,
            _ => AuctionEventType.AuctionApproved
        };

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = eventType,
            OccurredAt = DateTime.UtcNow,
            Notes = $"تحديث يدوي لحالة المزاد إلى {request.Status}."
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
