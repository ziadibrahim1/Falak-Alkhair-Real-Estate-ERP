using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Leads.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leads.Queries.GetLeadById;

public record GetLeadByIdQuery(Guid Id) : IRequest<LeadDto>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLeadByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .AsNoTracking()
            .Include(l => l.InterestedProperty)
            .Include(l => l.AssignedAgent)
            .Include(l => l.Campaign)
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lead), request.Id);

        return LeadDto.FromEntity(lead);
    }
}
