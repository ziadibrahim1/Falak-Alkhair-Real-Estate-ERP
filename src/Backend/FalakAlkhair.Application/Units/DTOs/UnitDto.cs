using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Units.DTOs;

public class UnitDto
{
    public Guid Id { get; set; }
    public string UnitCode { get; set; } = default!;
    public string UnitNumber { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public string? Floor { get; set; }
    public UnitType UnitType { get; set; }
    public UnitStatus CurrentStatus { get; set; }
    public decimal? Area { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasParking { get; set; }
    public decimal? RentalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? DepositAmount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public static UnitDto FromEntity(Unit u) => new()
    {
        Id = u.Id,
        UnitCode = u.UnitCode,
        UnitNumber = u.UnitNumber,
        PropertyId = u.PropertyId,
        PropertyName = u.Property?.PropertyName ?? string.Empty,
        Floor = u.Floor,
        UnitType = u.UnitType,
        CurrentStatus = u.CurrentStatus,
        Area = u.Area,
        Bedrooms = u.Bedrooms,
        Bathrooms = u.Bathrooms,
        IsFurnished = u.IsFurnished,
        HasParking = u.HasParking,
        RentalPrice = u.RentalPrice,
        SalePrice = u.SalePrice,
        DepositAmount = u.DepositAmount,
        Description = u.Description,
        CreatedAt = u.CreatedAt
    };
}
