using FalakAlkhair.Application.Auctions.DTOs;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Queries.GetAuctionsList;

public class GetAuctionsListQuery : ListQueryParams, IRequest<PaginatedList<AuctionDto>>
{
    public AuctionStatus? Status { get; init; }
}

public class GetAuctionsListQueryHandler : IRequestHandler<GetAuctionsListQuery, PaginatedList<AuctionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuctionsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<AuctionDto>> Handle(GetAuctionsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Auctions
            .AsNoTracking()
            .Include(a => a.Property)
            .Include(a => a.Unit)
            .Include(a => a.Owner)
            .Include(a => a.Agent)
            .Include(a => a.WinnerBuyer)
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a => a.AuctionNumber.Contains(term) || a.Property.PropertyName.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status.Value);

        query = request.SortDescending
            ? query.OrderByDescending(a => a.CreatedAt)
            : query.OrderBy(a => a.CreatedAt);

        var projected = query.Select(a => AuctionDto.FromEntity(a));

        return await PaginatedList<AuctionDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
