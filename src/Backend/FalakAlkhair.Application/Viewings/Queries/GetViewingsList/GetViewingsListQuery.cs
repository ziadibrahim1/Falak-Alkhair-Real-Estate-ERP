using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Viewings.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Viewings.Queries.GetViewingsList;

public class GetViewingsListQuery : ListQueryParams, IRequest<PaginatedList<ViewingDto>>
{
    public ViewingStatus? Status { get; init; }
    public Guid? AgentId { get; init; }
}

public class GetViewingsListQueryHandler : IRequestHandler<GetViewingsListQuery, PaginatedList<ViewingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetViewingsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ViewingDto>> Handle(GetViewingsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Viewings
            .AsNoTracking()
            .Include(v => v.Property)
            .Include(v => v.Unit)
            .Include(v => v.Buyer)
            .Include(v => v.Tenant)
            .Include(v => v.Agent)
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(v => v.ViewingCode.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(v => v.Status == request.Status.Value);
        if (request.AgentId.HasValue) query = query.Where(v => v.AgentId == request.AgentId.Value);

        query = request.SortDescending
            ? query.OrderByDescending(v => v.ScheduledAt)
            : query.OrderBy(v => v.ScheduledAt);

        var projected = query.Select(v => ViewingDto.FromEntity(v));

        return await PaginatedList<ViewingDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
