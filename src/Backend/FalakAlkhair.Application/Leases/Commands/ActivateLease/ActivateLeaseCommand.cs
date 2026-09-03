using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Commands.ActivateLease;

/// <summary>
/// تفعيل عقد الإيجار: Draft/PendingApproval → Active. يُحدّث حالة الوحدة
/// تلقائيًا إلى "مؤجرة" (Rented) كأثر جانبي حقيقي على حالة المخزون العقاري.
/// إن كان للعقد مسوّق عقاري (AgentId) ونسبة عمولة أكبر من صفر، تُولَّد
/// عمولة (Commission) تلقائيًا وفق CommissionPercentage المسجّلة على العقد.
/// </summary>
public record ActivateLeaseCommand(Guid Id) : IRequest;

public class ActivateLeaseCommandHandler : IRequestHandler<ActivateLeaseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public ActivateLeaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task Handle(ActivateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lease), request.Id);

        if (lease.Status is not (LeaseStatus.Draft or LeaseStatus.PendingApproval))
        {
            throw new Common.Exceptions.BusinessRuleException(
                $"لا يمكن تفعيل عقد بحالته الحالية ({lease.Status}). يجب أن يكون العقد في حالة مسودة أو بانتظار الاعتماد.");
        }

        var hasOverlappingActiveLease = await _context.Leases.AnyAsync(l =>
            l.Id != lease.Id && l.UnitId == lease.UnitId && !l.IsDeleted &&
            l.Status == LeaseStatus.Active &&
            l.StartDate <= lease.EndDate && l.EndDate >= lease.StartDate,
            cancellationToken);
        if (hasOverlappingActiveLease)
        {
            throw new Common.Exceptions.BusinessRuleException("هذه الوحدة مؤجرة بالفعل بعقد نشط آخر يتقاطع مع فترة هذا العقد.");
        }

        lease.Status = LeaseStatus.Active;
        lease.ActivatedAt = DateTime.UtcNow;

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == lease.UnitId, cancellationToken);
        if (unit is not null)
        {
            unit.CurrentStatus = Domain.Common.Enums.UnitStatus.Rented;
        }

        if (lease.AgentId.HasValue && lease.CommissionPercentage > 0)
        {
            var commissionExists = await _context.Commissions.AnyAsync(
                c => c.LeaseId == lease.Id && !c.IsDeleted, cancellationToken);

            if (!commissionExists)
            {
                const decimal vatPercentage = 15;
                var commissionAmount = Math.Round(lease.AnnualRentAmount * lease.CommissionPercentage / 100, 2, MidpointRounding.AwayFromZero);
                var vatAmount = Math.Round(commissionAmount * vatPercentage / 100, 2, MidpointRounding.AwayFromZero);

                var commissionNumber = await _numberGenerator.GenerateNextNumberAsync("COMM", lease.CompanyId, cancellationToken);

                _context.Commissions.Add(new Commission
                {
                    CompanyId = lease.CompanyId,
                    BranchId = lease.BranchId,
                    CommissionNumber = commissionNumber,
                    AgentId = lease.AgentId.Value,
                    SourceType = CommissionSourceType.Lease,
                    LeaseId = lease.Id,
                    BaseAmount = lease.AnnualRentAmount,
                    CommissionPercentage = lease.CommissionPercentage,
                    CommissionAmount = commissionAmount,
                    VatPercentage = vatPercentage,
                    VatAmount = vatAmount,
                    NetCommissionAmount = commissionAmount + vatAmount,
                    Status = CommissionStatus.Pending
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
