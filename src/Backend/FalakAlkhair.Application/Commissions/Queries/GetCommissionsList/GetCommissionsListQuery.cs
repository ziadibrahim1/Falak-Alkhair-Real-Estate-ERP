using FalakAlkhair.Application.Commissions.DTOs;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Commissions.Queries.GetCommissionsList;

public class GetCommissionsListQuery : ListQueryParams, IRequest<PaginatedList<CommissionDto>>
{
    public Guid? AgentId { get; init; }
    public CommissionStatus? Status { get; init; }
}

public class GetCommissionsListQueryHandler : IRequestHandler<GetCommissionsListQuery, PaginatedList<CommissionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCommissionsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<CommissionDto>> Handle(GetCommissionsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Commissions
            .AsNoTracking()
            .Include(c => c.Agent)
            .Include(c => c.Lease)
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c =>
                c.CommissionNumber.Contains(term) ||
                c.Agent.NameAr.Contains(term));
        }

        if (request.AgentId.HasValue)
        {
            query = query.Where(c => c.AgentId == request.AgentId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "amount" => request.SortDescending ? query.OrderByDescending(c => c.NetCommissionAmount) : query.OrderBy(c => c.NetCommissionAmount),
            _ => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var projected = query.Select(c => CommissionDto.FromEntity(c));

        return await PaginatedList<CommissionDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
