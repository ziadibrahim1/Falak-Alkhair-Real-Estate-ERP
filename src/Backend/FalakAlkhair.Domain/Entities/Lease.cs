using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عقد إيجار وحدة عقارية لمستأجر. يحكمه Workflow:
/// Draft → PendingApproval → Active → Terminated/Cancelled.
/// عند الإنشاء يُولَّد جدول سداد (LeasePayments) تلقائيًا حسب دورية السداد.
/// </summary>
public class Lease : BaseAuditableEntity
{
    public string LeaseNumber { get; set; } = default!; // LEASE-000001

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;

    /// <summary>مالك العقار وقت إنشاء العقد (منسوخ من Property.OwnerId لتسريع كشوف حساب الملاك).</summary>
    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = default!;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    /// <summary>المسوّق العقاري الذي أبرم العقد (اختياري). عند تفعيل العقد وتوفر
    /// مسوّق، تُولَّد عمولة (Commission) تلقائيًا وفق CommissionPercentage أدناه.</summary>
    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal AnnualRentAmount { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;
    public int NumberOfPayments { get; set; }

    public decimal SecurityDeposit { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal VatPercentage { get; set; } = 15;

    public LeaseStatus Status { get; set; } = LeaseStatus.Draft;

    public string? Notes { get; set; }

    public DateTime? ActivatedAt { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }

    public ICollection<LeasePayment> Payments { get; set; } = new List<LeasePayment>();
    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
