using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.SettleAuction;

/// <summary>تسوية مالية نهائية للمزاد: Awarded → Settled.</summary>
public record SettleAuctionCommand(Guid Id) : IRequest;

public class SettleAuctionCommandHandler : IRequestHandler<SettleAuctionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SettleAuctionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SettleAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        if (auction.Status != AuctionStatus.Awarded)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن تسوية مزاد بحالته الحالية ({auction.Status}). يجب إرساؤه أولًا.");
        }

        auction.Status = AuctionStatus.Settled;
        auction.SettledAt = DateTime.UtcNow;

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = AuctionEventType.AuctionSettled,
            OccurredAt = DateTime.UtcNow,
            Notes = "تمت التسوية المالية النهائية للمزاد."
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
