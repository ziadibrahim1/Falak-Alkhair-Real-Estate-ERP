using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عمولة مسوّق عقاري ناتجة عن معاملة فعلية. حاليًا يُدعَم مصدر واحد حقيقي
/// (عقد إيجار عبر LeaseId) لأن موديولات البيع/المزادات لم تُبنَ بعد
/// (راجع ROADMAP.md، Phase 5/7)؛ SourceType موجود مسبقًا ليبقى العقد
/// (Schema) جاهزًا لإضافة SaleId/AuctionId لاحقًا دون تعديل جوهري.
/// </summary>
public class Commission : BaseAuditableEntity
{
    public string CommissionNumber { get; set; } = default!; // COMM-000001

    public Guid AgentId { get; set; }
    public Agent Agent { get; set; } = default!;

    public CommissionSourceType SourceType { get; set; }

    public Guid? LeaseId { get; set; }
    public Lease? Lease { get; set; }

    public decimal BaseAmount { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal VatPercentage { get; set; } = 15;
    public decimal VatAmount { get; set; }
    public decimal NetCommissionAmount { get; set; }

    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }
}
