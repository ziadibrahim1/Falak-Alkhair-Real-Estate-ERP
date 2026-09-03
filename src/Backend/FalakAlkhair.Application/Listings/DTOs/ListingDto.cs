using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Listings.DTOs;

public class ListingDto
{
    public Guid Id { get; set; }
    public string ListingCode { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? Features { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public DateTime? ListingStartDate { get; set; }
    public DateTime? ListingEndDate { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ListingDto FromEntity(Listing listing) => new()
    {
        Id = listing.Id,
        ListingCode = listing.ListingCode,
        PropertyId = listing.PropertyId,
        PropertyName = listing.Property?.PropertyName ?? string.Empty,
        UnitId = listing.UnitId,
        UnitNumber = listing.Unit?.UnitNumber ?? string.Empty,
        ListingType = listing.ListingType,
        Price = listing.Price,
        Description = listing.Description,
        Features = listing.Features,
        AgentId = listing.AgentId,
        AgentNameAr = listing.Agent?.NameAr,
        ListingStartDate = listing.ListingStartDate,
        ListingEndDate = listing.ListingEndDate,
        Status = listing.Status,
        CreatedAt = listing.CreatedAt
    };
}
