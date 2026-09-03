using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// بائع (Seller CRM) — يمثّل تفويض بيع (Sale Mandate) من مالك (Owner) على
/// عقار محدد. لا يُدمَج مع Owner لأن نفس المالك قد يكون له أكثر من تفويض
/// بيع نشط على عقارات مختلفة بشروط تسعير مختلفة في نفس الوقت.
/// </summary>
public class Seller : BaseAuditableEntity
{
    public string SellerCode { get; set; } = default!; // SELLER-000001

    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = default!;

    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }

    public decimal AskingPrice { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal CommissionPercentage { get; set; }

    public ListingMandateStatus MandateStatus { get; set; } = ListingMandateStatus.Draft;
    public DateTime MandateStartDate { get; set; }
    public DateTime? MandateEndDate { get; set; }

    public Guid? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public string? Notes { get; set; }
}
