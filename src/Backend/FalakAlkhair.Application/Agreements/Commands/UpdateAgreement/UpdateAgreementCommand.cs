using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agreements.Commands.UpdateAgreement;

public record UpdateAgreementCommand : IRequest
{
    public Guid Id { get; init; }
    public DateTime EndDate { get; init; }
    public decimal ManagementFee { get; init; }
    public decimal CommissionPercentage { get; init; }
    public string? PaymentTerms { get; init; }
    public string? Responsibilities { get; init; }
    public string? RenewalTerms { get; init; }
    public string? TerminationTerms { get; init; }
    public ManagementAgreementStatus Status { get; init; }
}

public class UpdateAgreementCommandValidator : AbstractValidator<UpdateAgreementCommand>
{
    public UpdateAgreementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateAgreementCommandHandler : IRequestHandler<UpdateAgreementCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAgreementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAgreementCommand request, CancellationToken cancellationToken)
    {
        var agreement = await _context.PropertyManagementAgreements
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.PropertyManagementAgreement), request.Id);

        if (agreement.Status == ManagementAgreementStatus.Terminated)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن تعديل عقد تم إنهاؤه.");
        }

        agreement.EndDate = request.EndDate;
        agreement.ManagementFee = request.ManagementFee;
        agreement.CommissionPercentage = request.CommissionPercentage;
        agreement.PaymentTerms = request.PaymentTerms;
        agreement.Responsibilities = request.Responsibilities;
        agreement.RenewalTerms = request.RenewalTerms;
        agreement.TerminationTerms = request.TerminationTerms;
        agreement.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
