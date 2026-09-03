using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Commands.TerminateLease;

/// <summary>إنهاء عقد إيجار نشط: Active → Terminated. يعيد حالة الوحدة إلى "متاحة".</summary>
public record TerminateLeaseCommand(Guid Id, string? Reason) : IRequest;

public class TerminateLeaseCommandValidator : AbstractValidator<TerminateLeaseCommand>
{
    public TerminateLeaseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class TerminateLeaseCommandHandler : IRequestHandler<TerminateLeaseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TerminateLeaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(TerminateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lease), request.Id);

        if (lease.Status != LeaseStatus.Active)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن إنهاء عقد غير نشط.");
        }

        lease.Status = LeaseStatus.Terminated;
        lease.TerminatedAt = DateTime.UtcNow;
        lease.TerminationReason = request.Reason;

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == lease.UnitId, cancellationToken);
        if (unit is not null && unit.CurrentStatus == Domain.Common.Enums.UnitStatus.Rented)
        {
            unit.CurrentStatus = Domain.Common.Enums.UnitStatus.Available;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
