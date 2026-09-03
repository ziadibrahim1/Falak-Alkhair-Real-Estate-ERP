using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Commands.CreateLease;

public record CreateLeaseCommand : IRequest<Guid>
{
    public Guid TenantId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal AnnualRentAmount { get; init; }
    public PaymentFrequency PaymentFrequency { get; init; } = PaymentFrequency.Annual;
    public decimal SecurityDeposit { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal VatPercentage { get; init; } = 15;
    public string? Notes { get; init; }
}

public class CreateLeaseCommandValidator : AbstractValidator<CreateLeaseCommand>
{
    public CreateLeaseCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        RuleFor(x => x.AnnualRentAmount).GreaterThan(0).WithMessage("قيمة الإيجار السنوي يجب أن تكون أكبر من صفر.");
        RuleFor(x => x.SecurityDeposit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
    }
}

public class CreateLeaseCommandHandler : IRequestHandler<CreateLeaseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateLeaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateLeaseCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == request.TenantId && t.CompanyId == companyId && !t.IsDeleted, cancellationToken);
        if (!tenantExists) throw new NotFoundException(nameof(Tenant), request.TenantId);

        var property = await _context.Properties
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Property), request.PropertyId);

        var unit = await _context.Units
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(FalakAlkhair.Domain.Entities.Unit), request.UnitId);

        if (unit.PropertyId != request.PropertyId)
        {
            throw new Common.Exceptions.BusinessRuleException("الوحدة المحددة لا تنتمي للعقار المحدد.");
        }

        if (request.AgentId.HasValue)
        {
            var agentExists = await _context.Agents.AnyAsync(
                a => a.Id == request.AgentId.Value && a.CompanyId == companyId && !a.IsDeleted, cancellationToken);
            if (!agentExists) throw new NotFoundException(nameof(Domain.Entities.Agent), request.AgentId.Value);
        }

        // منع ازدواج تأجير نفس الوحدة بعقد نشط آخر يتقاطع في الفترة الزمنية.
        var hasOverlappingActiveLease = await _context.Leases.AnyAsync(l =>
            l.UnitId == request.UnitId && !l.IsDeleted &&
            l.Status == LeaseStatus.Active &&
            l.StartDate <= request.EndDate && l.EndDate >= request.StartDate,
            cancellationToken);
        if (hasOverlappingActiveLease)
        {
            throw new Common.Exceptions.BusinessRuleException("هذه الوحدة مؤجرة بالفعل بعقد نشط يتقاطع مع الفترة المحددة.");
        }

        var numberOfPayments = request.PaymentFrequency switch
        {
            PaymentFrequency.Monthly => 12,
            PaymentFrequency.Quarterly => 4,
            PaymentFrequency.SemiAnnual => 2,
            PaymentFrequency.Annual => 1,
            _ => 1
        };

        var code = await _numberGenerator.GenerateNextNumberAsync("LEASE", companyId, cancellationToken);

        var lease = new Lease
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            LeaseNumber = code,
            TenantId = request.TenantId,
            OwnerId = property.OwnerId,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            AgentId = request.AgentId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AnnualRentAmount = request.AnnualRentAmount,
            PaymentFrequency = request.PaymentFrequency,
            NumberOfPayments = numberOfPayments,
            SecurityDeposit = request.SecurityDeposit,
            CommissionPercentage = request.CommissionPercentage,
            VatPercentage = request.VatPercentage,
            Notes = request.Notes,
            Status = LeaseStatus.Draft
        };

        // توليد جدول السداد تلقائيًا: مبلغ متساوٍ لكل قسط، مع تحميل فرق التقريب
        // (إن وجد) على القسط الأخير حتى يتطابق مجموع الأقساط تمامًا مع الإيجار السنوي.
        var monthsPerInstallment = 12 / numberOfPayments;
        var baseInstallmentAmount = Math.Round(request.AnnualRentAmount / numberOfPayments, 2, MidpointRounding.AwayFromZero);
        var runningTotal = 0m;

        for (var i = 1; i <= numberOfPayments; i++)
        {
            var isLast = i == numberOfPayments;
            var installmentAmount = isLast ? request.AnnualRentAmount - runningTotal : baseInstallmentAmount;
            runningTotal += installmentAmount;

            lease.Payments.Add(new LeasePayment
            {
                CompanyId = companyId,
                BranchId = _currentUser.BranchId,
                InstallmentNumber = i,
                DueDate = request.StartDate.AddMonths((i - 1) * monthsPerInstallment),
                Amount = installmentAmount,
                PaidAmount = 0,
                Status = LeasePaymentStatus.Pending
            });
        }

        _context.Leases.Add(lease);
        await _context.SaveChangesAsync(cancellationToken);

        return lease.Id;
    }
}
