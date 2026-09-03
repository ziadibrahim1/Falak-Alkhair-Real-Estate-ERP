using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Listings.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Listings.Queries.GetListingsList;

public class GetListingsListQuery : ListQueryParams, IRequest<PaginatedList<ListingDto>>
{
    public ListingStatus? Status { get; init; }
    public ListingType? ListingType { get; init; }
}

public class GetListingsListQueryHandler : IRequestHandler<GetListingsListQuery, PaginatedList<ListingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetListingsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ListingDto>> Handle(GetListingsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Property)
            .Include(l => l.Unit)
            .Include(l => l.Agent)
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(l =>
                l.ListingCode.Contains(term) ||
                l.Property.PropertyName.Contains(term) ||
                l.Unit.UnitNumber.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(l => l.Status == request.Status.Value);
        if (request.ListingType.HasValue) query = query.Where(l => l.ListingType == request.ListingType.Value);

        query = request.SortDescending
            ? query.OrderByDescending(l => l.CreatedAt)
            : query.OrderBy(l => l.CreatedAt);

        var projected = query.Select(l => ListingDto.FromEntity(l));

        return await PaginatedList<ListingDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
