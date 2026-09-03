using FalakAlkhair.Application.Buyers.DTOs;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Buyers.Queries.GetBuyersList;

public class GetBuyersListQuery : ListQueryParams, IRequest<PaginatedList<BuyerDto>>
{
    public bool? IsActive { get; init; }
}

public class GetBuyersListQueryHandler : IRequestHandler<GetBuyersListQuery, PaginatedList<BuyerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBuyersListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<BuyerDto>> Handle(GetBuyersListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Buyers
            .AsNoTracking()
            .Include(b => b.AssignedAgent)
            .Where(b => b.CompanyId == _currentUser.CompanyId && !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(b =>
                b.NameAr.Contains(term) ||
                (b.NameEn != null && b.NameEn.Contains(term)) ||
                b.BuyerCode.Contains(term) ||
                b.Mobile.Contains(term));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(b => b.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(b => b.NameAr) : query.OrderBy(b => b.NameAr),
            "code" => request.SortDescending ? query.OrderByDescending(b => b.BuyerCode) : query.OrderBy(b => b.BuyerCode),
            _ => request.SortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt)
        };

        var projected = query.Select(b => BuyerDto.FromEntity(b));

        return await PaginatedList<BuyerDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
