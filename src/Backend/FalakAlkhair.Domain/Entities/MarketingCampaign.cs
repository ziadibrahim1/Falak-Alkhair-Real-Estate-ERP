using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// حملة تسويقية عقارية. الأداء (Leads/Conversions) يُحسب من العملاء المحتملين
/// (Lead) المرتبطين فعليًا بهذه الحملة عبر Lead.CampaignId، وليس عدّادًا يدويًا.
/// </summary>
public class MarketingCampaign : BaseAuditableEntity
{
    public string CampaignCode { get; set; } = default!; // CAMP-000001

    public string Name { get; set; } = default!;
    public MarketingChannel Channel { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }

    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }

    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
