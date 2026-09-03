using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceEmployees.Commands.UpdateMaintenanceEmployee;

public record UpdateMaintenanceEmployeeCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? Department { get; init; }
    public string? Skills { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateMaintenanceEmployeeCommandValidator : AbstractValidator<UpdateMaintenanceEmployeeCommand>
{
    public UpdateMaintenanceEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateMaintenanceEmployeeCommandHandler : IRequestHandler<UpdateMaintenanceEmployeeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateMaintenanceEmployeeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMaintenanceEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.MaintenanceEmployees
            .Where(e => e.CompanyId == _currentUser.CompanyId && !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceEmployee), request.Id);

        employee.NameAr = request.NameAr;
        employee.Mobile = request.Mobile;
        employee.Email = request.Email;
        employee.Department = request.Department;
        employee.Skills = request.Skills;
        employee.IsAvailable = request.IsAvailable;
        employee.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
