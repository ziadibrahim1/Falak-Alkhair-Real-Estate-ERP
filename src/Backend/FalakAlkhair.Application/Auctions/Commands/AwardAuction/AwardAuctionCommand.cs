using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.AwardAuction;

/// <summary>
/// إرساء المزاد على فائز: Ended → Awarded. يولّد عمولة (Commission) تلقائيًا
/// (إن وُجد مسوّق ونسبة عمولة > صفر) بنفس فلسفة تفعيل عقد الإيجار/إتمام البيع.
/// </summary>
public record AwardAuctionCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid WinnerBuyerId { get; init; }
    public decimal FinalPrice { get; init; }
}

public class AwardAuctionCommandValidator : AbstractValidator<AwardAuctionCommand>
{
    public AwardAuctionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.WinnerBuyerId).NotEmpty();
        RuleFor(x => x.FinalPrice).GreaterThan(0).WithMessage("السعر النهائي يجب أن يكون أكبر من صفر.");
    }
}

public class AwardAuctionCommandHandler : IRequestHandler<AwardAuctionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;
    private readonly INotificationService _notifications;

    public AwardAuctionCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator, INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
        _notifications = notifications;
    }

    public async Task Handle(AwardAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        if (auction.Status != AuctionStatus.Ended)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن إرساء مزاد بحالته الحالية ({auction.Status}). يجب أن ينتهي أولًا.");
        }

        if (auction.ReservePrice.HasValue && request.FinalPrice < auction.ReservePrice.Value)
        {
            throw new Common.Exceptions.BusinessRuleException("السعر النهائي أقل من السعر الاحتياطي للمزاد.");
        }

        var buyerExists = await _context.Buyers.AnyAsync(
            b => b.Id == request.WinnerBuyerId && b.CompanyId == _currentUser.CompanyId && !b.IsDeleted, cancellationToken);
        if (!buyerExists) throw new NotFoundException(nameof(Domain.Entities.Buyer), request.WinnerBuyerId);

        auction.Status = AuctionStatus.Awarded;
        auction.WinnerBuyerId = request.WinnerBuyerId;
        auction.FinalPrice = request.FinalPrice;

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = AuctionEventType.AuctionAwarded,
            OccurredAt = DateTime.UtcNow,
            Notes = $"أُرسي المزاد بسعر نهائي {request.FinalPrice:N2}."
        });

        _notifications.Notify(
            auction.CompanyId,
            auction.BranchId,
            userId: null,
            Domain.Common.Enums.NotificationType.AuctionAwarded,
            "تم إرساء مزاد",
            $"أُرسي المزاد \"{auction.AuctionNumber}\" بسعر نهائي {request.FinalPrice:N2}.",
            link: "/auctions");

        if (auction.AgentId.HasValue && auction.CommissionPercentage > 0)
        {
            var commissionExists = await _context.Commissions.AnyAsync(c => c.AuctionId == auction.Id && !c.IsDeleted, cancellationToken);
            if (!commissionExists)
            {
                var commissionAmount = Math.Round(request.FinalPrice * auction.CommissionPercentage / 100, 2, MidpointRounding.AwayFromZero);
                var vatAmount = Math.Round(commissionAmount * auction.VatPercentage / 100, 2, MidpointRounding.AwayFromZero);
                var commissionNumber = await _numberGenerator.GenerateNextNumberAsync("COMM", auction.CompanyId, cancellationToken);

                _context.Commissions.Add(new Commission
                {
                    CompanyId = auction.CompanyId,
                    BranchId = auction.BranchId,
                    CommissionNumber = commissionNumber,
                    AgentId = auction.AgentId.Value,
                    SourceType = CommissionSourceType.Auction,
                    AuctionId = auction.Id,
                    BaseAmount = request.FinalPrice,
                    CommissionPercentage = auction.CommissionPercentage,
                    CommissionAmount = commissionAmount,
                    VatPercentage = auction.VatPercentage,
                    VatAmount = vatAmount,
                    NetCommissionAmount = commissionAmount + vatAmount,
                    Status = CommissionStatus.Pending
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
