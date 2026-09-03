using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Agents.Commands.CreateAgent;

public record CreateAgentCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string? NationalId { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? FalLicenseNumber { get; init; }
    public DateTime? FalLicenseExpiryDate { get; init; }
    public string? Specialization { get; init; }
    public Guid? ManagerUserId { get; init; }
    public CommissionType CommissionSchemeType { get; init; } = CommissionType.Percentage;
    public decimal DefaultCommissionPercentage { get; init; }
    public decimal? DefaultCommissionFixedAmount { get; init; }
    public string? Notes { get; init; }
}

public class CreateAgentCommandValidator : AbstractValidator<CreateAgentCommand>
{
    public CreateAgentCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المسوّق بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
        RuleFor(x => x.DefaultCommissionPercentage).InclusiveBetween(0, 100)
            .WithMessage("نسبة العمولة يجب أن تكون بين 0 و100.");
    }
}

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateAgentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("AGENT", companyId, cancellationToken);

        var agent = new Agent
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            AgentCode = code,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            NationalId = request.NationalId,
            Mobile = request.Mobile,
            Email = request.Email,
            FalLicenseNumber = request.FalLicenseNumber,
            FalLicenseExpiryDate = request.FalLicenseExpiryDate,
            Specialization = request.Specialization,
            ManagerUserId = request.ManagerUserId,
            Status = AgentStatus.Active,
            CommissionSchemeType = request.CommissionSchemeType,
            DefaultCommissionPercentage = request.DefaultCommissionPercentage,
            DefaultCommissionFixedAmount = request.DefaultCommissionFixedAmount,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        return agent.Id;
    }
}
