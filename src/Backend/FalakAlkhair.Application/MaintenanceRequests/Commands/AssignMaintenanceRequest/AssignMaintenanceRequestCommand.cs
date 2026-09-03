using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Commands.AssignMaintenanceRequest;

/// <summary>إسناد طلب صيانة لفني داخلي و/أو مورّد خارجي: New → Assigned.</summary>
public record AssignMaintenanceRequestCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? VendorId { get; init; }
}

public class AssignMaintenanceRequestCommandValidator : AbstractValidator<AssignMaintenanceRequestCommand>
{
    public AssignMaintenanceRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x).Must(x => x.EmployeeId.HasValue || x.VendorId.HasValue)
            .WithMessage("يجب تحديد فني داخلي أو مورّد خارجي على الأقل.")
            .WithName("EmployeeId");
    }
}

public class AssignMaintenanceRequestCommandHandler : IRequestHandler<AssignMaintenanceRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AssignMaintenanceRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .Where(r => r.CompanyId == _currentUser.CompanyId && !r.IsDeleted)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceRequest), request.Id);

        if (maintenanceRequest.Status is not (MaintenanceStatus.New or MaintenanceStatus.Assigned))
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن إسناد طلب بحالته الحالية ({maintenanceRequest.Status}).");
        }

        if (request.EmployeeId.HasValue)
        {
            var employeeExists = await _context.MaintenanceEmployees.AnyAsync(
                e => e.Id == request.EmployeeId.Value && e.CompanyId == _currentUser.CompanyId && !e.IsDeleted, cancellationToken);
            if (!employeeExists) throw new NotFoundException(nameof(Domain.Entities.MaintenanceEmployee), request.EmployeeId.Value);
        }

        if (request.VendorId.HasValue)
        {
            var vendorExists = await _context.Vendors.AnyAsync(
                v => v.Id == request.VendorId.Value && v.CompanyId == _currentUser.CompanyId && !v.IsDeleted, cancellationToken);
            if (!vendorExists) throw new NotFoundException(nameof(Domain.Entities.Vendor), request.VendorId.Value);
        }

        maintenanceRequest.AssignedEmployeeId = request.EmployeeId;
        maintenanceRequest.AssignedVendorId = request.VendorId;
        maintenanceRequest.Status = MaintenanceStatus.Assigned;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
