using FalakAlkhair.Application.Auctions.DTOs;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Queries.GetAuctionAuditLogs;

/// <summary>سجل تدقيق كامل لمزاد محدَّد، مرتَّب زمنيًا (الأحدث أولًا) — للقراءة فقط، لا يوجد أمر تعديل مقابل له.</summary>
public record GetAuctionAuditLogsQuery(Guid AuctionId) : IRequest<List<AuctionAuditLogDto>>;

public class GetAuctionAuditLogsQueryHandler : IRequestHandler<GetAuctionAuditLogsQuery, List<AuctionAuditLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuctionAuditLogsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AuctionAuditLogDto>> Handle(GetAuctionAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AuctionAuditLogs
            .AsNoTracking()
            .Where(l => l.AuctionId == request.AuctionId && l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .OrderByDescending(l => l.OccurredAt)
            .Select(l => AuctionAuditLogDto.FromEntity(l))
            .ToListAsync(cancellationToken);
    }
}
