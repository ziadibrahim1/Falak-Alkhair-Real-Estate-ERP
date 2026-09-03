using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.MaintenanceEmployees.Commands.CreateMaintenanceEmployee;

public record CreateMaintenanceEmployeeCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = default!;
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? Department { get; init; }
    public string? Skills { get; init; }
}

public class CreateMaintenanceEmployeeCommandValidator : AbstractValidator<CreateMaintenanceEmployeeCommand>
{
    public CreateMaintenanceEmployeeCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم الفني بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class CreateMaintenanceEmployeeCommandHandler : IRequestHandler<CreateMaintenanceEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateMaintenanceEmployeeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateMaintenanceEmployeeCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("EMP", companyId, cancellationToken);

        var employee = new MaintenanceEmployee
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            EmployeeCode = code,
            NameAr = request.NameAr,
            Mobile = request.Mobile,
            Email = request.Email,
            Department = request.Department,
            Skills = request.Skills,
            IsAvailable = true,
            IsActive = true
        };

        _context.MaintenanceEmployees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
