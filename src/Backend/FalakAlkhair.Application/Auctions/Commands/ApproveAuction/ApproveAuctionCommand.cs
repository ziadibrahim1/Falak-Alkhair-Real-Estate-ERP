using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.ApproveAuction;

/// <summary>اعتماد المزاد: Draft/PendingApproval → Scheduled.</summary>
public record ApproveAuctionCommand(Guid Id) : IRequest;

public class ApproveAuctionCommandHandler : IRequestHandler<ApproveAuctionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveAuctionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        if (auction.Status is not (AuctionStatus.Draft or AuctionStatus.PendingApproval))
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن اعتماد مزاد بحالته الحالية ({auction.Status}).");
        }

        auction.Status = AuctionStatus.Scheduled;

        _context.AuctionAuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = auction.CompanyId,
            BranchId = auction.BranchId,
            AuctionId = auction.Id,
            EventType = AuctionEventType.AuctionApproved,
            OccurredAt = DateTime.UtcNow,
            Notes = "تم اعتماد المزاد وجدولته."
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
