using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>وحدة عقارية تابعة لعقار (شقة، مكتب، محل ...).</summary>
public class Unit : BaseAuditableEntity
{
    public string UnitCode { get; set; } = default!; // UNIT-000001
    public string UnitNumber { get; set; } = default!;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string? Floor { get; set; }
    public UnitType UnitType { get; set; }
    public UnitStatus CurrentStatus { get; set; } = UnitStatus.Available;

    public decimal? Area { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasParking { get; set; }

    public string? ElectricityMeterNumber { get; set; }
    public string? WaterMeterNumber { get; set; }
    public AcType? AcType { get; set; }

    public decimal? RentalPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? DepositAmount { get; set; }

    public string? Description { get; set; }
}
