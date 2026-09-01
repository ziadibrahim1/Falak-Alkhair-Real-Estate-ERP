using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Units.Commands.DeleteUnit;

public record DeleteUnitCommand(Guid Id) : IRequest;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteUnitCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units
            .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        if (unit.CurrentStatus is UnitStatus.Rented or UnitStatus.Sold)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف وحدة مؤجرة أو مباعة.");
        }

        unit.IsDeleted = true;
        unit.DeletedAt = DateTime.UtcNow;
        unit.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
