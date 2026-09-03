using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Buyers.DTOs;

public class BuyerDto
{
    public Guid Id { get; set; }
    public string BuyerCode { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? NationalId { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public decimal? Budget { get; set; }
    public string? PreferredCity { get; set; }
    public string? PreferredDistrict { get; set; }
    public PropertyType? PreferredPropertyType { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public BuyerPurpose Purpose { get; set; }
    public FinancingStatus FinancingStatus { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentNameAr { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public static BuyerDto FromEntity(Buyer buyer) => new()
    {
        Id = buyer.Id,
        BuyerCode = buyer.BuyerCode,
        NameAr = buyer.NameAr,
        NameEn = buyer.NameEn,
        NationalId = buyer.NationalId,
        Mobile = buyer.Mobile,
        Email = buyer.Email,
        Budget = buyer.Budget,
        PreferredCity = buyer.PreferredCity,
        PreferredDistrict = buyer.PreferredDistrict,
        PreferredPropertyType = buyer.PreferredPropertyType,
        MinArea = buyer.MinArea,
        MaxArea = buyer.MaxArea,
        Purpose = buyer.Purpose,
        FinancingStatus = buyer.FinancingStatus,
        AssignedAgentId = buyer.AssignedAgentId,
        AssignedAgentNameAr = buyer.AssignedAgent?.NameAr,
        Notes = buyer.Notes,
        IsActive = buyer.IsActive,
        CreatedAt = buyer.CreatedAt
    };
}

/// <summary>عقار/وحدة مرشّحة لمشترٍ حسب محرك المطابقة البسيط (Buyer-Property Matching).</summary>
public class PropertyMatchDto
{
    public Guid PropertyId { get; set; }
    public string PropertyCode { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public PropertyType PropertyType { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public decimal? TotalArea { get; set; }

    public Guid UnitId { get; set; }
    public string UnitCode { get; set; } = default!;
    public string UnitNumber { get; set; } = default!;
    public decimal? Area { get; set; }
    public decimal? SalePrice { get; set; }
}
