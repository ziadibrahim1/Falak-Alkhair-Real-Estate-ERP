using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Tenants.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTenantByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .Include(t => t.Leases)
            .Where(t => t.CompanyId == _currentUser.CompanyId && !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Tenant), request.Id);

        return TenantDto.FromEntity(tenant);
    }
}
