using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Buyers.Commands.CreateBuyer;

public record CreateBuyerCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string? NationalId { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public decimal? Budget { get; init; }
    public string? PreferredCity { get; init; }
    public string? PreferredDistrict { get; init; }
    public PropertyType? PreferredPropertyType { get; init; }
    public decimal? MinArea { get; init; }
    public decimal? MaxArea { get; init; }
    public BuyerPurpose Purpose { get; init; } = BuyerPurpose.PersonalUse;
    public FinancingStatus FinancingStatus { get; init; } = FinancingStatus.Undetermined;
    public Guid? AssignedAgentId { get; init; }
    public string? Notes { get; init; }
}

public class CreateBuyerCommandValidator : AbstractValidator<CreateBuyerCommand>
{
    public CreateBuyerCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المشتري بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
        RuleFor(x => x.MaxArea).GreaterThanOrEqualTo(x => x.MinArea)
            .When(x => x.MinArea.HasValue && x.MaxArea.HasValue)
            .WithMessage("الحد الأقصى للمساحة يجب أن يكون أكبر من أو يساوي الحد الأدنى.");
    }
}

public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateBuyerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("BUYER", companyId, cancellationToken);

        var buyer = new Buyer
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            BuyerCode = code,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            NationalId = request.NationalId,
            Mobile = request.Mobile,
            Email = request.Email,
            Budget = request.Budget,
            PreferredCity = request.PreferredCity,
            PreferredDistrict = request.PreferredDistrict,
            PreferredPropertyType = request.PreferredPropertyType,
            MinArea = request.MinArea,
            MaxArea = request.MaxArea,
            Purpose = request.Purpose,
            FinancingStatus = request.FinancingStatus,
            AssignedAgentId = request.AssignedAgentId,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Buyers.Add(buyer);
        await _context.SaveChangesAsync(cancellationToken);

        return buyer.Id;
    }
}
