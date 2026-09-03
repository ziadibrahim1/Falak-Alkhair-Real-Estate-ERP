using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceEmployees.Commands.DeleteMaintenanceEmployee;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteMaintenanceEmployeeCommand(Guid Id) : IRequest;

public class DeleteMaintenanceEmployeeCommandHandler : IRequestHandler<DeleteMaintenanceEmployeeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteMaintenanceEmployeeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteMaintenanceEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.MaintenanceEmployees
            .Where(e => e.CompanyId == _currentUser.CompanyId && !e.IsDeleted)
            .Include(e => e.AssignedRequests)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceEmployee), request.Id);

        if (employee.AssignedRequests.Any(r => !r.IsDeleted && r.Status != Domain.Common.Enums.MaintenanceStatus.Completed && r.Status != Domain.Common.Enums.MaintenanceStatus.Cancelled))
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف فني لديه طلبات صيانة مسندة غير مكتملة.");
        }

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
