using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Viewings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Viewings.Queries.GetViewingById;

public record GetViewingByIdQuery(Guid Id) : IRequest<ViewingDto>;

public class GetViewingByIdQueryHandler : IRequestHandler<GetViewingByIdQuery, ViewingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetViewingByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ViewingDto> Handle(GetViewingByIdQuery request, CancellationToken cancellationToken)
    {
        var viewing = await _context.Viewings
            .AsNoTracking()
            .Include(v => v.Property)
            .Include(v => v.Unit)
            .Include(v => v.Buyer)
            .Include(v => v.Tenant)
            .Include(v => v.Agent)
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Viewing), request.Id);

        return ViewingDto.FromEntity(viewing);
    }
}
