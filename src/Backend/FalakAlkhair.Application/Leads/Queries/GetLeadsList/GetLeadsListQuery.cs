using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Leads.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leads.Queries.GetLeadsList;

public class GetLeadsListQuery : ListQueryParams, IRequest<PaginatedList<LeadDto>>
{
    public LeadStatus? Status { get; init; }
    public LeadType? LeadType { get; init; }
    public Guid? AssignedAgentId { get; init; }
}

public class GetLeadsListQueryHandler : IRequestHandler<GetLeadsListQuery, PaginatedList<LeadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLeadsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<LeadDto>> Handle(GetLeadsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Include(l => l.InterestedProperty)
            .Include(l => l.AssignedAgent)
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(l =>
                l.NameAr.Contains(term) ||
                l.LeadCode.Contains(term) ||
                l.Mobile.Contains(term));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        if (request.LeadType.HasValue)
        {
            query = query.Where(l => l.LeadType == request.LeadType.Value);
        }

        if (request.AssignedAgentId.HasValue)
        {
            query = query.Where(l => l.AssignedAgentId == request.AssignedAgentId.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "priority" => request.SortDescending ? query.OrderByDescending(l => l.Priority) : query.OrderBy(l => l.Priority),
            "code" => request.SortDescending ? query.OrderByDescending(l => l.LeadCode) : query.OrderBy(l => l.LeadCode),
            _ => request.SortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt)
        };

        var projected = query.Select(l => LeadDto.FromEntity(l));

        return await PaginatedList<LeadDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
