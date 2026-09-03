using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عرض سعر صيانة مقدَّم من مورّد لطلب صيانة محدَّد. يدعم تعدُّد العروض على
/// نفس الطلب للمقارنة؛ اعتماد أحدها يرفض بقية العروض المعلَّقة تلقائيًا.
/// </summary>
public class MaintenanceQuotation : BaseAuditableEntity
{
    public string QuotationNumber { get; set; } = default!; // QUOT-000001

    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = default!;

    public Guid MaintenanceRequestId { get; set; }
    public MaintenanceRequest MaintenanceRequest { get; set; } = default!;

    public DateTime? ValidUntil { get; set; }
    public decimal VatPercentage { get; set; } = 15;
    public decimal SubtotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public QuotationStatus Status { get; set; } = QuotationStatus.Pending;
    public string? Notes { get; set; }

    public ICollection<MaintenanceQuotationItem> Items { get; set; } = new List<MaintenanceQuotationItem>();
}

/// <summary>بند ضمن عرض سعر صيانة (كمية × سعر الوحدة).</summary>
public class MaintenanceQuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public MaintenanceQuotation Quotation { get; set; } = default!;

    public string Description { get; set; } = default!;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
