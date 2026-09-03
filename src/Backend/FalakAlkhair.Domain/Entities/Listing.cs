using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// إعلان عقاري (Listing) لوحدة محدَّدة، للبيع أو للإيجار. عند النشر (Publish)
/// تتحدَّث حالة الوحدة (UnitStatus) تلقائيًا إلى ListedForSale/ListedForRent.
/// </summary>
public class Listing : BaseAuditableEntity
{
    public string ListingCode { get; set; } = default!; // LIST-000001

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }

    public string? Description { get; set; }
    public string? Features { get; set; }

    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public DateTime? ListingStartDate { get; set; }
    public DateTime? ListingEndDate { get; set; }

    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    public ICollection<Viewing> Viewings { get; set; } = new List<Viewing>();
}
