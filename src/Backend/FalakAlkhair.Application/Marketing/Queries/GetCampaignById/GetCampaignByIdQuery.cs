using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Marketing.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Marketing.Queries.GetCampaignById;

public record GetCampaignByIdQuery(Guid Id) : IRequest<MarketingCampaignDto>;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, MarketingCampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCampaignByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MarketingCampaignDto> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _context.MarketingCampaigns
            .AsNoTracking()
            .Include(c => c.Property)
            .Include(c => c.Agent)
            .Include(c => c.Leads)
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MarketingCampaign), request.Id);

        return MarketingCampaignDto.FromEntity(campaign);
    }
}
