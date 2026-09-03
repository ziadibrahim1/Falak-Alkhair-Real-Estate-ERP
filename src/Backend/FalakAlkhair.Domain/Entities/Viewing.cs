using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>معاينة عقار/وحدة من قِبل مشترٍ أو مستأجر محتمل.</summary>
public class Viewing : BaseAuditableEntity
{
    public string ViewingCode { get; set; } = default!; // VIEW-000001

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public Guid? ListingId { get; set; }
    public Listing? Listing { get; set; }

    /// <summary>أحدهما مطلوب: مشترٍ محتمل أو مستأجر محتمل يعاين الوحدة.</summary>
    public Guid? BuyerId { get; set; }
    public Buyer? Buyer { get; set; }

    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public DateTime ScheduledAt { get; set; }
    public ViewingStatus Status { get; set; } = ViewingStatus.Scheduled;

    public string? Notes { get; set; }
    public string? Feedback { get; set; }
}
