using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Tenants.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Tenants.Queries.GetTenantsList;

public class GetTenantsListQuery : ListQueryParams, IRequest<PaginatedList<TenantDto>>
{
    public bool? IsActive { get; init; }
}

public class GetTenantsListQueryHandler : IRequestHandler<GetTenantsListQuery, PaginatedList<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTenantsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<TenantDto>> Handle(GetTenantsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants
            .AsNoTracking()
            .Where(t => t.CompanyId == _currentUser.CompanyId && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(t => t.NameAr.Contains(term) || t.Mobile.Contains(term) || t.TenantCode.Contains(term));
        }

        if (request.IsActive.HasValue) query = query.Where(t => t.IsActive == request.IsActive);

        query = request.SortDescending
            ? query.OrderByDescending(t => t.CreatedAt)
            : query.OrderBy(t => t.CreatedAt);

        var projected = query.Select(t => new TenantDto
        {
            Id = t.Id,
            TenantCode = t.TenantCode,
            PartyType = t.PartyType,
            NameAr = t.NameAr,
            NameEn = t.NameEn,
            NationalId = t.NationalId,
            CommercialRegistrationNumber = t.CommercialRegistrationNumber,
            Mobile = t.Mobile,
            Email = t.Email,
            NationalAddress = t.NationalAddress,
            City = t.City,
            District = t.District,
            Employer = t.Employer,
            Notes = t.Notes,
            IsActive = t.IsActive,
            LeasesCount = t.Leases.Count(l => !l.IsDeleted),
            CreatedAt = t.CreatedAt
        });

        return await PaginatedList<TenantDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
