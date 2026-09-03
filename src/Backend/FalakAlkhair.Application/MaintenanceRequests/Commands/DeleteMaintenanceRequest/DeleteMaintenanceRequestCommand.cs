using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Commands.DeleteMaintenanceRequest;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteMaintenanceRequestCommand(Guid Id) : IRequest;

public class DeleteMaintenanceRequestCommandHandler : IRequestHandler<DeleteMaintenanceRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteMaintenanceRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .Where(r => r.CompanyId == _currentUser.CompanyId && !r.IsDeleted)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceRequest), request.Id);

        maintenanceRequest.IsDeleted = true;
        maintenanceRequest.DeletedAt = DateTime.UtcNow;
        maintenanceRequest.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
