using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Sellers.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sellers.Queries.GetSellersList;

public class GetSellersListQuery : ListQueryParams, IRequest<PaginatedList<SellerDto>>
{
    public ListingMandateStatus? MandateStatus { get; init; }
}

public class GetSellersListQueryHandler : IRequestHandler<GetSellersListQuery, PaginatedList<SellerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSellersListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<SellerDto>> Handle(GetSellersListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sellers
            .AsNoTracking()
            .Include(s => s.Owner)
            .Include(s => s.Property)
            .Include(s => s.AssignedAgent)
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(s =>
                s.SellerCode.Contains(term) ||
                s.Owner.NameAr.Contains(term) ||
                (s.Property != null && s.Property.PropertyName.Contains(term)));
        }

        if (request.MandateStatus.HasValue)
        {
            query = query.Where(s => s.MandateStatus == request.MandateStatus.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(s => s.AskingPrice) : query.OrderBy(s => s.AskingPrice),
            "code" => request.SortDescending ? query.OrderByDescending(s => s.SellerCode) : query.OrderBy(s => s.SellerCode),
            _ => request.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        var projected = query.Select(s => SellerDto.FromEntity(s));

        return await PaginatedList<SellerDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
