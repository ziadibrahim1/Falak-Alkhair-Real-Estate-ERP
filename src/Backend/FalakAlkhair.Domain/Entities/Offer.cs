using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>عرض شراء مقدَّم من مشترٍ محتمل على وحدة معروضة للبيع. يدعم تعدُّد العروض على نفس الوحدة.</summary>
public class Offer : BaseAuditableEntity
{
    public string OfferNumber { get; set; } = default!; // OFFER-000001

    public Guid BuyerId { get; set; }
    public Buyer Buyer { get; set; } = default!;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = default!;

    public decimal Amount { get; set; }
    public DateTime OfferDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public string? Conditions { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Pending;

    public string? Notes { get; set; }
}
