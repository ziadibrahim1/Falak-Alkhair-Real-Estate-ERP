namespace FalakAlkhair.Application.Dashboard.DTOs;

public class DashboardStatsDto
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public int AvailableUnits { get; set; }
    public int RentedUnits { get; set; }
    public int SoldUnits { get; set; }
    public int ListedUnits { get; set; }

    public int ActiveLeases { get; set; }
    public decimal ActiveLeasesAnnualRentValue { get; set; }

    public int OverduePaymentsCount { get; set; }
    public decimal OverduePaymentsAmount { get; set; }

    public int OpenMaintenanceRequests { get; set; }
    public int UrgentMaintenanceRequests { get; set; }

    public int UpcomingAuctions { get; set; }
    public int LiveAuctions { get; set; }

    public int SalesPipelineCount { get; set; }
    public decimal SalesPipelineValue { get; set; }
    public int SalesCompletedThisMonth { get; set; }
    public decimal SalesCompletedThisMonthValue { get; set; }

    public decimal PendingCommissionsAmount { get; set; }

    public int TotalLeads { get; set; }
    public int NewLeadsThisMonth { get; set; }
}
