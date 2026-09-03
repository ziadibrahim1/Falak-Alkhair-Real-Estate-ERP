using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceEmployees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceEmployees.Queries.GetMaintenanceEmployeesList;

public class GetMaintenanceEmployeesListQuery : ListQueryParams, IRequest<PaginatedList<MaintenanceEmployeeDto>>
{
    public bool? IsAvailable { get; init; }
}

public class GetMaintenanceEmployeesListQueryHandler : IRequestHandler<GetMaintenanceEmployeesListQuery, PaginatedList<MaintenanceEmployeeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMaintenanceEmployeesListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<MaintenanceEmployeeDto>> Handle(GetMaintenanceEmployeesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaintenanceEmployees
            .AsNoTracking()
            .Include(e => e.AssignedRequests)
            .Where(e => e.CompanyId == _currentUser.CompanyId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(e => e.NameAr.Contains(term) || e.EmployeeCode.Contains(term) || e.Mobile.Contains(term));
        }

        if (request.IsAvailable.HasValue) query = query.Where(e => e.IsAvailable == request.IsAvailable.Value);

        query = request.SortDescending
            ? query.OrderByDescending(e => e.CreatedAt)
            : query.OrderBy(e => e.CreatedAt);

        var projected = query.Select(e => MaintenanceEmployeeDto.FromEntity(e));

        return await PaginatedList<MaintenanceEmployeeDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
