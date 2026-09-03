using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Sales.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sales.Queries.GetSalesList;

public class GetSalesListQuery : ListQueryParams, IRequest<PaginatedList<SaleDto>>
{
    public SaleStage? Stage { get; init; }
}

public class GetSalesListQueryHandler : IRequestHandler<GetSalesListQuery, PaginatedList<SaleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSalesListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<SaleDto>> Handle(GetSalesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Include(s => s.Property)
            .Include(s => s.Unit)
            .Include(s => s.Seller)
            .Include(s => s.Buyer)
            .Include(s => s.Agent)
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(s => s.SaleNumber.Contains(term) || s.Buyer.NameAr.Contains(term));
        }

        if (request.Stage.HasValue) query = query.Where(s => s.Stage == request.Stage.Value);

        query = request.SortDescending
            ? query.OrderByDescending(s => s.CreatedAt)
            : query.OrderBy(s => s.CreatedAt);

        var projected = query.Select(s => SaleDto.FromEntity(s));

        return await PaginatedList<SaleDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
