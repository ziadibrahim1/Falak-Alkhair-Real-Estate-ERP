using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Vendors.Commands.CreateVendor;

public record CreateVendorCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = default!;
    public string? ContactPerson { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? CommercialRegistrationNumber { get; init; }
    public string? VatNumber { get; init; }
    public string? Services { get; init; }
}

public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المورّد بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateVendorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("VEND", companyId, cancellationToken);

        var vendor = new Vendor
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            VendorCode = code,
            NameAr = request.NameAr,
            ContactPerson = request.ContactPerson,
            Mobile = request.Mobile,
            Email = request.Email,
            CommercialRegistrationNumber = request.CommercialRegistrationNumber,
            VatNumber = request.VatNumber,
            Services = request.Services,
            IsActive = true
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        return vendor.Id;
    }
}
