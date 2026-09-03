using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.MaintenanceRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Queries.GetMaintenanceRequestById;

public record GetMaintenanceRequestByIdQuery(Guid Id) : IRequest<MaintenanceRequestDto>;

public class GetMaintenanceRequestByIdQueryHandler : IRequestHandler<GetMaintenanceRequestByIdQuery, MaintenanceRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMaintenanceRequestByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MaintenanceRequestDto> Handle(GetMaintenanceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .AsNoTracking()
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Tenant)
            .Include(r => r.AssignedEmployee)
            .Include(r => r.AssignedVendor)
            .Where(r => r.CompanyId == _currentUser.CompanyId && !r.IsDeleted)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceRequest), request.Id);

        return MaintenanceRequestDto.FromEntity(maintenanceRequest);
    }
}
