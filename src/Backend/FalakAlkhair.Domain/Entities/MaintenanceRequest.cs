using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// طلب صيانة على عقار/وحدة، بدورة عمل كاملة:
/// New → Assigned → Inspection → Quotation → WaitingApproval → Approved →
/// InProgress → WaitingParts → Completed/Cancelled.
/// </summary>
public class MaintenanceRequest : BaseAuditableEntity
{
    public string RequestNumber { get; set; } = default!; // MAINT-000001

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public MaintenanceRequestType RequestType { get; set; }
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public string Description { get; set; } = default!;

    public Guid? AssignedEmployeeId { get; set; }
    public MaintenanceEmployee? AssignedEmployee { get; set; }

    public Guid? AssignedVendorId { get; set; }
    public Vendor? AssignedVendor { get; set; }

    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }

    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.New;

    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    public ICollection<MaintenanceQuotation> Quotations { get; set; } = new List<MaintenanceQuotation>();
}
