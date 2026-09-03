using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<Guid>
{
    public PartyType PartyType { get; init; }
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string? NationalId { get; init; }
    public string? CommercialRegistrationNumber { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? NationalAddress { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public string? Employer { get; init; }
    public string? Notes { get; init; }
}

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المستأجر بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
        RuleFor(x => x.NationalId).NotEmpty().When(x => x.PartyType == PartyType.Individual)
            .WithMessage("رقم الهوية/الإقامة مطلوب للأفراد.");
        RuleFor(x => x.CommercialRegistrationNumber).NotEmpty().When(x => x.PartyType == PartyType.Company)
            .WithMessage("رقم السجل التجاري مطلوب للشركات.");
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateTenantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("TEN", companyId, cancellationToken);

        var tenant = new Tenant
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            TenantCode = code,
            PartyType = request.PartyType,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            NationalId = request.NationalId,
            CommercialRegistrationNumber = request.CommercialRegistrationNumber,
            Mobile = request.Mobile,
            Email = request.Email,
            NationalAddress = request.NationalAddress,
            City = request.City,
            District = request.District,
            Employer = request.Employer,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
