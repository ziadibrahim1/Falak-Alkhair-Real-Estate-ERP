using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Vendors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Vendors.Queries.GetVendorsList;

public class GetVendorsListQuery : ListQueryParams, IRequest<PaginatedList<VendorDto>>
{
    public bool? IsActive { get; init; }
}

public class GetVendorsListQueryHandler : IRequestHandler<GetVendorsListQuery, PaginatedList<VendorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetVendorsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<VendorDto>> Handle(GetVendorsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vendors
            .AsNoTracking()
            .Include(v => v.AssignedRequests)
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(v => v.NameAr.Contains(term) || v.VendorCode.Contains(term) || v.Mobile.Contains(term));
        }

        if (request.IsActive.HasValue) query = query.Where(v => v.IsActive == request.IsActive.Value);

        query = request.SortDescending
            ? query.OrderByDescending(v => v.CreatedAt)
            : query.OrderBy(v => v.CreatedAt);

        var projected = query.Select(v => VendorDto.FromEntity(v));

        return await PaginatedList<VendorDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
