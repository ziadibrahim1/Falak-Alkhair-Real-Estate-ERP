using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Properties.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public string PropertyCode { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public PropertyType PropertyType { get; set; }
    public PropertyCategory PropertyCategory { get; set; }
    public PropertyStatus Status { get; set; }

    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;

    public string? DeedNumber { get; set; }
    public DateTime? DeedDate { get; set; }

    public string? City { get; set; }
    public string? District { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? NationalAddressShortCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public decimal? TotalArea { get; set; }
    public decimal? BuildingArea { get; set; }
    public int? NumberOfFloors { get; set; }
    public int? YearBuilt { get; set; }
    public string? Description { get; set; }

    public int UnitsCount { get; set; }
    public int AvailableUnitsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static PropertyDto FromEntity(Property p) => new()
    {
        Id = p.Id,
        PropertyCode = p.PropertyCode,
        PropertyName = p.PropertyName,
        PropertyType = p.PropertyType,
        PropertyCategory = p.PropertyCategory,
        Status = p.Status,
        OwnerId = p.OwnerId,
        OwnerNameAr = p.Owner?.NameAr ?? string.Empty,
        DeedNumber = p.DeedNumber,
        DeedDate = p.DeedDate,
        City = p.City,
        District = p.District,
        Street = p.Street,
        BuildingNumber = p.BuildingNumber,
        NationalAddressShortCode = p.NationalAddressShortCode,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        TotalArea = p.TotalArea,
        BuildingArea = p.BuildingArea,
        NumberOfFloors = p.NumberOfFloors,
        YearBuilt = p.YearBuilt,
        Description = p.Description,
        UnitsCount = p.Units?.Count(u => !u.IsDeleted) ?? 0,
        AvailableUnitsCount = p.Units?.Count(u => !u.IsDeleted && u.CurrentStatus == UnitStatus.Available) ?? 0,
        CreatedAt = p.CreatedAt
    };
}
