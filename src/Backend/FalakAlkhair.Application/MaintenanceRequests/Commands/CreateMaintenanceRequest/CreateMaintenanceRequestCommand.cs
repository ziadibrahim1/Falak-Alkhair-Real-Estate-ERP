using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

public record CreateMaintenanceRequestCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public Guid? TenantId { get; init; }
    public MaintenanceRequestType RequestType { get; init; }
    public MaintenancePriority Priority { get; init; } = MaintenancePriority.Medium;
    public string Description { get; init; } = default!;
}

public class CreateMaintenanceRequestCommandValidator : AbstractValidator<CreateMaintenanceRequestCommand>
{
    public CreateMaintenanceRequestCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().WithMessage("وصف طلب الصيانة مطلوب.").MaximumLength(2000);
    }
}

public class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateMaintenanceRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var unit = await _context.Units
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        if (unit.PropertyId != request.PropertyId)
        {
            throw new Common.Exceptions.BusinessRuleException("الوحدة المحددة لا تنتمي للعقار المحدد.");
        }

        var property = await _context.Properties.FirstAsync(p => p.Id == request.PropertyId, cancellationToken);

        var code = await _numberGenerator.GenerateNextNumberAsync("MAINT", companyId, cancellationToken);

        var maintenanceRequest = new MaintenanceRequest
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            RequestNumber = code,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            TenantId = request.TenantId,
            OwnerId = property.OwnerId,
            RequestType = request.RequestType,
            Priority = request.Priority,
            Description = request.Description,
            Status = MaintenanceStatus.New
        };

        _context.MaintenanceRequests.Add(maintenanceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return maintenanceRequest.Id;
    }
}
