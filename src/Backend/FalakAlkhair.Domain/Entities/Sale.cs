using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// معاملة بيع تمر بمسار مبيعات (Sales Pipeline): Lead → Qualified → Viewing →
/// Offer → Negotiation → Reserved → Contract → Payment → Completed/Cancelled.
/// عند الوصول لمرحلة Completed تُولَّد عمولة (Commission) تلقائيًا وتتحدَّث
/// حالة الوحدة إلى Sold، بنفس فلسفة تفعيل عقد الإيجار في Phase 3/4.
/// </summary>
public class Sale : BaseAuditableEntity
{
    public string SaleNumber { get; set; } = default!; // SALE-000001

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public Guid SellerId { get; set; }
    public Seller Seller { get; set; } = default!;

    public Guid BuyerId { get; set; }
    public Buyer Buyer { get; set; } = default!;

    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public Guid? OfferId { get; set; }
    public Offer? Offer { get; set; }

    public decimal AskingPrice { get; set; }
    public decimal FinalPrice { get; set; }

    public decimal CommissionPercentage { get; set; }
    public decimal VatPercentage { get; set; } = 15;

    public SaleStage Stage { get; set; } = SaleStage.Lead;

    public DateTime? CompletedAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }

    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
