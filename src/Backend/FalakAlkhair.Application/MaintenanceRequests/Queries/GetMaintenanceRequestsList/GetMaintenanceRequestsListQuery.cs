using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceRequests.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Queries.GetMaintenanceRequestsList;

public class GetMaintenanceRequestsListQuery : ListQueryParams, IRequest<PaginatedList<MaintenanceRequestDto>>
{
    public MaintenanceStatus? Status { get; init; }
    public MaintenancePriority? Priority { get; init; }
}

public class GetMaintenanceRequestsListQueryHandler : IRequestHandler<GetMaintenanceRequestsListQuery, PaginatedList<MaintenanceRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMaintenanceRequestsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<MaintenanceRequestDto>> Handle(GetMaintenanceRequestsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaintenanceRequests
            .AsNoTracking()
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Tenant)
            .Include(r => r.AssignedEmployee)
            .Include(r => r.AssignedVendor)
            .Where(r => r.CompanyId == _currentUser.CompanyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(r => r.RequestNumber.Contains(term) || r.Description.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(r => r.Status == request.Status.Value);
        if (request.Priority.HasValue) query = query.Where(r => r.Priority == request.Priority.Value);

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "priority" => request.SortDescending ? query.OrderByDescending(r => r.Priority) : query.OrderBy(r => r.Priority),
            _ => request.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
        };

        var projected = query.Select(r => MaintenanceRequestDto.FromEntity(r));

        return await PaginatedList<MaintenanceRequestDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
