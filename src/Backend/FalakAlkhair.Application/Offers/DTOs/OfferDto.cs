using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Offers.DTOs;

public class OfferDto
{
    public Guid Id { get; set; }
    public string OfferNumber { get; set; } = default!;
    public Guid BuyerId { get; set; }
    public string BuyerNameAr { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime OfferDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Conditions { get; set; }
    public OfferStatus Status { get; set; }
    public string? Notes { get; set; }

    public static OfferDto FromEntity(Offer offer) => new()
    {
        Id = offer.Id,
        OfferNumber = offer.OfferNumber,
        BuyerId = offer.BuyerId,
        BuyerNameAr = offer.Buyer?.NameAr ?? string.Empty,
        PropertyId = offer.PropertyId,
        PropertyName = offer.Property?.PropertyName ?? string.Empty,
        UnitId = offer.UnitId,
        UnitNumber = offer.Unit?.UnitNumber ?? string.Empty,
        Amount = offer.Amount,
        OfferDate = offer.OfferDate,
        ExpirationDate = offer.ExpirationDate,
        Conditions = offer.Conditions,
        Status = offer.Status,
        Notes = offer.Notes
    };
}
