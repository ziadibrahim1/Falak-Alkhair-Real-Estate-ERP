using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Reports.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetOwnerStatement;

public record GetOwnerStatementQuery(Guid OwnerId, DateTime? From, DateTime? To) : IRequest<OwnerStatementDto>;

public class GetOwnerStatementQueryHandler : IRequestHandler<GetOwnerStatementQuery, OwnerStatementDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOwnerStatementQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OwnerStatementDto> Handle(GetOwnerStatementQuery request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var owner = await _context.Owners
            .AsNoTracking()
            .Where(o => o.CompanyId == companyId && !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Owner), request.OwnerId);

        var periodTo = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var periodFrom = (request.From ?? periodTo.AddYears(-1)).Date;

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.Lease.OwnerId == request.OwnerId &&
                        p.PaymentDate >= periodFrom && p.PaymentDate <= periodTo)
            .Select(p => new
            {
                p.PaymentDate,
                p.PaymentNumber,
                p.Amount,
                LeaseNumber = p.Lease.LeaseNumber,
                PropertyName = p.Lease.Property.PropertyName,
                p.Lease.PropertyId,
                CommissionPercentage = p.Lease.CommissionPercentage
            })
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        var rentalIncome = payments.Sum(p => p.Amount);

        // أتعاب الإدارة: النسبة المئوية المتفق عليها في عقد الإيجار نفسه لكل دفعة إيجار محصّلة.
        // (يُستخدم عقد الإيجار لأنه المصدر الفعلي للتحصيل؛ عقد إدارة الأملاك يُستخدم لاحقًا
        // عند تفعيل موديول تسوية الملاك الكامل.)
        var managementFees = payments.Sum(p => Math.Round(p.Amount * p.CommissionPercentage / 100m, 2));

        const decimal maintenanceExpenses = 0; // يُفعَّل مع موديول الصيانة (Phase 6)
        const decimal otherExpenses = 0;
        const decimal openingBalance = 0; // يُفعَّل مع بنية الترحيل المحاسبي الكاملة (البند 39)
        const decimal paymentsToOwner = 0; // يُفعَّل مع موديول تسوية الملاك المالي

        var netOwnerAmount = rentalIncome - managementFees - maintenanceExpenses - otherExpenses;
        var closingBalance = openingBalance + netOwnerAmount - paymentsToOwner;

        return new OwnerStatementDto
        {
            OwnerId = owner.Id,
            OwnerNameAr = owner.NameAr,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            OpeningBalance = openingBalance,
            RentalIncome = rentalIncome,
            ManagementFees = managementFees,
            MaintenanceExpenses = maintenanceExpenses,
            OtherExpenses = otherExpenses,
            NetOwnerAmount = netOwnerAmount,
            PaymentsToOwner = paymentsToOwner,
            ClosingBalance = closingBalance,
            Lines = payments.Select(p => new OwnerStatementLineDto
            {
                PaymentDate = p.PaymentDate,
                LeaseNumber = p.LeaseNumber,
                PropertyName = p.PropertyName,
                PaymentNumber = p.PaymentNumber,
                Amount = p.Amount
            }).ToList()
        };
    }
}
