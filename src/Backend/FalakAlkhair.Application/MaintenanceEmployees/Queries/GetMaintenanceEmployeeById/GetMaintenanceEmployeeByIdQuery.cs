using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.MaintenanceEmployees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceEmployees.Queries.GetMaintenanceEmployeeById;

public record GetMaintenanceEmployeeByIdQuery(Guid Id) : IRequest<MaintenanceEmployeeDto>;

public class GetMaintenanceEmployeeByIdQueryHandler : IRequestHandler<GetMaintenanceEmployeeByIdQuery, MaintenanceEmployeeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMaintenanceEmployeeByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MaintenanceEmployeeDto> Handle(GetMaintenanceEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.MaintenanceEmployees
            .AsNoTracking()
            .Include(e => e.AssignedRequests)
            .Where(e => e.CompanyId == _currentUser.CompanyId && !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceEmployee), request.Id);

        return MaintenanceEmployeeDto.FromEntity(employee);
    }
}
