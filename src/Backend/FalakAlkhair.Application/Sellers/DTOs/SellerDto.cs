using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Sellers.DTOs;

public class SellerDto
{
    public Guid Id { get; set; }
    public string SellerCode { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;
    public Guid? PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public decimal AskingPrice { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal CommissionPercentage { get; set; }
    public ListingMandateStatus MandateStatus { get; set; }
    public DateTime MandateStartDate { get; set; }
    public DateTime? MandateEndDate { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentNameAr { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static SellerDto FromEntity(Seller seller) => new()
    {
        Id = seller.Id,
        SellerCode = seller.SellerCode,
        OwnerId = seller.OwnerId,
        OwnerNameAr = seller.Owner?.NameAr ?? string.Empty,
        PropertyId = seller.PropertyId,
        PropertyName = seller.Property?.PropertyName,
        AskingPrice = seller.AskingPrice,
        MinimumPrice = seller.MinimumPrice,
        CommissionPercentage = seller.CommissionPercentage,
        MandateStatus = seller.MandateStatus,
        MandateStartDate = seller.MandateStartDate,
        MandateEndDate = seller.MandateEndDate,
        AssignedAgentId = seller.AssignedAgentId,
        AssignedAgentNameAr = seller.AssignedAgent?.NameAr,
        Notes = seller.Notes,
        CreatedAt = seller.CreatedAt
    };
}
