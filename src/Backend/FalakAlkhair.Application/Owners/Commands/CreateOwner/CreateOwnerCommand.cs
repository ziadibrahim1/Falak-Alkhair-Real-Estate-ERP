using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Owners.Commands.CreateOwner;

public record CreateOwnerCommand : IRequest<Guid>
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
    public string? BankName { get; init; }
    public string? Iban { get; init; }
    public string? Notes { get; init; }
}

public class CreateOwnerCommandValidator : AbstractValidator<CreateOwnerCommand>
{
    public CreateOwnerCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المالك بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
        RuleFor(x => x.NationalId).NotEmpty().When(x => x.PartyType == PartyType.Individual)
            .WithMessage("رقم الهوية/الإقامة مطلوب للأفراد.");
        RuleFor(x => x.CommercialRegistrationNumber).NotEmpty().When(x => x.PartyType == PartyType.Company)
            .WithMessage("رقم السجل التجاري مطلوب للشركات.");
        RuleFor(x => x.Iban).Matches(@"^SA[0-9]{22}$").When(x => !string.IsNullOrWhiteSpace(x.Iban))
            .WithMessage("رقم الآيبان يجب أن يبدأ بـ SA ويتكون من 24 خانة.");
    }
}

public class CreateOwnerCommandHandler : IRequestHandler<CreateOwnerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateOwnerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateOwnerCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("OWNER", companyId, cancellationToken);

        var owner = new Owner
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            OwnerCode = code,
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
            BankName = request.BankName,
            Iban = request.Iban,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync(cancellationToken);

        return owner.Id;
    }
}
