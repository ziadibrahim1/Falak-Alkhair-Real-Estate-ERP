using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Dashboard.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// إحصائيات لوحة التحكم المجمَّعة على الخادم بأمر واحد (وليس عبر عدّة نداءات
/// من الواجهة الأمامية لكل قائمة على حِدة كما كان يحدث سابقًا في صفحة
/// /dashboard قبل هذه المرحلة) — أداء أفضل ونتائج صحيحة (بدل حقول ثابتة/مكرَّرة).
/// </summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var units = _context.Units.AsNoTracking().Where(u => u.CompanyId == companyId && !u.IsDeleted);
        var leases = _context.Leases.AsNoTracking().Where(l => l.CompanyId == companyId && !l.IsDeleted);
        var leasePayments = _context.LeasePayments.AsNoTracking().Where(p => p.CompanyId == companyId && !p.IsDeleted);
        var maintenanceRequests = _context.MaintenanceRequests.AsNoTracking().Where(m => m.CompanyId == companyId && !m.IsDeleted);
        var auctions = _context.Auctions.AsNoTracking().Where(a => a.CompanyId == companyId && !a.IsDeleted);
        var sales = _context.Sales.AsNoTracking().Where(s => s.CompanyId == companyId && !s.IsDeleted);
        var commissions = _context.Commissions.AsNoTracking().Where(c => c.CompanyId == companyId && !c.IsDeleted);
        var leads = _context.Leads.AsNoTracking().Where(l => l.CompanyId == companyId && !l.IsDeleted);

        var dto = new DashboardStatsDto
        {
            TotalProperties = await _context.Properties.AsNoTracking().CountAsync(p => p.CompanyId == companyId && !p.IsDeleted, cancellationToken),

            TotalUnits = await units.CountAsync(cancellationToken),
            AvailableUnits = await units.CountAsync(u => u.CurrentStatus == UnitStatus.Available, cancellationToken),
            RentedUnits = await units.CountAsync(u => u.CurrentStatus == UnitStatus.Rented, cancellationToken),
            SoldUnits = await units.CountAsync(u => u.CurrentStatus == UnitStatus.Sold, cancellationToken),
            ListedUnits = await units.CountAsync(u => u.CurrentStatus == UnitStatus.ListedForSale || u.CurrentStatus == UnitStatus.ListedForRent, cancellationToken),

            ActiveLeases = await leases.CountAsync(l => l.Status == LeaseStatus.Active, cancellationToken),
            ActiveLeasesAnnualRentValue = await leases.Where(l => l.Status == LeaseStatus.Active).SumAsync(l => (decimal?)l.AnnualRentAmount, cancellationToken) ?? 0,

            OverduePaymentsCount = await leasePayments.CountAsync(
                p => p.Status != LeasePaymentStatus.Paid && p.Status != LeasePaymentStatus.Cancelled && p.DueDate < now, cancellationToken),
            OverduePaymentsAmount = await leasePayments
                .Where(p => p.Status != LeasePaymentStatus.Paid && p.Status != LeasePaymentStatus.Cancelled && p.DueDate < now)
                .SumAsync(p => (decimal?)(p.Amount - p.PaidAmount), cancellationToken) ?? 0,

            OpenMaintenanceRequests = await maintenanceRequests.CountAsync(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled, cancellationToken),
            UrgentMaintenanceRequests = await maintenanceRequests.CountAsync(
                m => (m.Priority == MaintenancePriority.High || m.Priority == MaintenancePriority.Critical)
                    && m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled, cancellationToken),

            UpcomingAuctions = await auctions.CountAsync(a => a.Status == AuctionStatus.Scheduled || a.Status == AuctionStatus.Published, cancellationToken),
            LiveAuctions = await auctions.CountAsync(a => a.Status == AuctionStatus.Live, cancellationToken),

            SalesPipelineCount = await sales.CountAsync(s => s.Stage != SaleStage.Completed && s.Stage != SaleStage.Cancelled, cancellationToken),
            SalesPipelineValue = await sales
                .Where(s => s.Stage != SaleStage.Completed && s.Stage != SaleStage.Cancelled)
                .SumAsync(s => (decimal?)s.AskingPrice, cancellationToken) ?? 0,
            SalesCompletedThisMonth = await sales.CountAsync(s => s.Stage == SaleStage.Completed && s.CompletedAt >= monthStart, cancellationToken),
            SalesCompletedThisMonthValue = await sales
                .Where(s => s.Stage == SaleStage.Completed && s.CompletedAt >= monthStart)
                .SumAsync(s => (decimal?)s.FinalPrice, cancellationToken) ?? 0,

            PendingCommissionsAmount = await commissions
                .Where(c => c.Status == CommissionStatus.Pending)
                .SumAsync(c => (decimal?)c.NetCommissionAmount, cancellationToken) ?? 0,

            TotalLeads = await leads.CountAsync(cancellationToken),
            NewLeadsThisMonth = await leads.CountAsync(l => l.CreatedAt >= monthStart, cancellationToken)
        };

        return dto;
    }
}
