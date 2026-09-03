using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Marketing.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Marketing.Queries.GetCampaignsList;

public class GetCampaignsListQuery : ListQueryParams, IRequest<PaginatedList<MarketingCampaignDto>>
{
    public bool? IsActive { get; init; }
}

public class GetCampaignsListQueryHandler : IRequestHandler<GetCampaignsListQuery, PaginatedList<MarketingCampaignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCampaignsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<MarketingCampaignDto>> Handle(GetCampaignsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MarketingCampaigns
            .AsNoTracking()
            .Include(c => c.Property)
            .Include(c => c.Agent)
            .Include(c => c.Leads)
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.CampaignCode.Contains(term));
        }

        if (request.IsActive.HasValue) query = query.Where(c => c.IsActive == request.IsActive.Value);

        query = request.SortDescending
            ? query.OrderByDescending(c => c.CreatedAt)
            : query.OrderBy(c => c.CreatedAt);

        var projected = query.Select(c => MarketingCampaignDto.FromEntity(c));

        return await PaginatedList<MarketingCampaignDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
