using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetRentRollReport;

public class RentRollLineDto
{
    public string LeaseNumber { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public string UnitNumber { get; set; } = default!;
    public string TenantNameAr { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AnnualRentAmount { get; set; }
    public string PaymentFrequency { get; set; } = default!;
    public DateTime? NextDueDate { get; set; }
}

/// <summary>تقرير عقود الإيجار السارية (Rent Roll) — كل عقد Active مع أقرب دفعة مستحقة.</summary>
public record GetRentRollReportQuery : IRequest<List<RentRollLineDto>>;

public class GetRentRollReportQueryHandler : IRequestHandler<GetRentRollReportQuery, List<RentRollLineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRentRollReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RentRollLineDto>> Handle(GetRentRollReportQuery request, CancellationToken cancellationToken)
    {
        var leases = await _context.Leases
            .AsNoTracking()
            .Include(l => l.Property)
            .Include(l => l.Unit)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted && l.Status == LeaseStatus.Active)
            .ToListAsync(cancellationToken);

        return leases.Select(l => new RentRollLineDto
        {
            LeaseNumber = l.LeaseNumber,
            PropertyName = l.Property.PropertyName,
            UnitNumber = l.Unit.UnitNumber,
            TenantNameAr = l.Tenant.NameAr,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            AnnualRentAmount = l.AnnualRentAmount,
            PaymentFrequency = l.PaymentFrequency.ToString(),
            NextDueDate = l.Payments
                .Where(p => p.Status != LeasePaymentStatus.Paid && p.Status != LeasePaymentStatus.Cancelled)
                .OrderBy(p => p.DueDate)
                .Select(p => (DateTime?)p.DueDate)
                .FirstOrDefault()
        }).ToList();
    }
}
