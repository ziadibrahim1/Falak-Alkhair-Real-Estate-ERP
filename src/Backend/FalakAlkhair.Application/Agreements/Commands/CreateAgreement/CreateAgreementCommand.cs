using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agreements.Commands.CreateAgreement;

public record CreateAgreementCommand : IRequest<Guid>
{
    public Guid OwnerId { get; init; }
    public Guid PropertyId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal ManagementFee { get; init; }
    public CommissionType CommissionType { get; init; }
    public decimal CommissionPercentage { get; init; }
    public string? PaymentTerms { get; init; }
    public string? Responsibilities { get; init; }
    public string? RenewalTerms { get; init; }
    public string? TerminationTerms { get; init; }
}

public class CreateAgreementCommandValidator : AbstractValidator<CreateAgreementCommand>
{
    public CreateAgreementCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100).When(x => x.CommissionType == CommissionType.Percentage);
        RuleFor(x => x.ManagementFee).GreaterThanOrEqualTo(0);
    }
}

public class CreateAgreementCommandHandler : IRequestHandler<CreateAgreementCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateAgreementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateAgreementCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var ownerExists = await _context.Owners.AnyAsync(o => o.Id == request.OwnerId && o.CompanyId == companyId && !o.IsDeleted, cancellationToken);
        if (!ownerExists) throw new NotFoundException(nameof(Owner), request.OwnerId);

        var propertyExists = await _context.Properties.AnyAsync(p => p.Id == request.PropertyId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);
        if (!propertyExists) throw new NotFoundException(nameof(Property), request.PropertyId);

        var code = await _numberGenerator.GenerateNextNumberAsync("PMA", companyId, cancellationToken);

        var agreement = new PropertyManagementAgreement
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            ContractNumber = code,
            OwnerId = request.OwnerId,
            PropertyId = request.PropertyId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ManagementFee = request.ManagementFee,
            CommissionType = request.CommissionType,
            CommissionPercentage = request.CommissionPercentage,
            PaymentTerms = request.PaymentTerms,
            Responsibilities = request.Responsibilities,
            RenewalTerms = request.RenewalTerms,
            TerminationTerms = request.TerminationTerms,
            Status = ManagementAgreementStatus.Draft
        };

        _context.PropertyManagementAgreements.Add(agreement);
        await _context.SaveChangesAsync(cancellationToken);

        return agreement.Id;
    }
}
