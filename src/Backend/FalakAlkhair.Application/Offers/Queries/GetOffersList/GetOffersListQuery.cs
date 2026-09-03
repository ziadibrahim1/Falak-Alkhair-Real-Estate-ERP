using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Offers.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Offers.Queries.GetOffersList;

public class GetOffersListQuery : ListQueryParams, IRequest<PaginatedList<OfferDto>>
{
    public OfferStatus? Status { get; init; }
    public Guid? UnitId { get; init; }
}

public class GetOffersListQueryHandler : IRequestHandler<GetOffersListQuery, PaginatedList<OfferDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOffersListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OfferDto>> Handle(GetOffersListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Offers
            .AsNoTracking()
            .Include(o => o.Buyer)
            .Include(o => o.Property)
            .Include(o => o.Unit)
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o => o.OfferNumber.Contains(term) || o.Buyer.NameAr.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(o => o.Status == request.Status.Value);
        if (request.UnitId.HasValue) query = query.Where(o => o.UnitId == request.UnitId.Value);

        query = request.SortDescending
            ? query.OrderByDescending(o => o.Amount)
            : query.OrderBy(o => o.CreatedAt);

        var projected = query.Select(o => OfferDto.FromEntity(o));

        return await PaginatedList<OfferDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
