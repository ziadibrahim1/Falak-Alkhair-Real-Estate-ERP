using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>عقار (مبنى/أرض/فيلا ...) قد يحتوي على وحدات (Units) متعددة.</summary>
public class Property : BaseAuditableEntity
{
    public string PropertyCode { get; set; } = default!; // PROP-000001
    public string PropertyName { get; set; } = default!;

    public PropertyType PropertyType { get; set; }
    public PropertyCategory PropertyCategory { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Active;

    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = default!;

    // بيانات الملكية والصك
    public string? DeedNumber { get; set; }
    public DateTime? DeedDate { get; set; }
    public string? OwnershipDocumentPath { get; set; }

    // العنوان الوطني السعودي
    public string? NationalAddressShortCode { get; set; } // مثال: RRRD1234
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? AdditionalNumber { get; set; }
    public string? PostalCode { get; set; }

    // الإحداثيات الجغرافية
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // المساحات والمواصفات
    public decimal? TotalArea { get; set; }
    public decimal? BuildingArea { get; set; }
    public int? NumberOfFloors { get; set; }
    public int? YearBuilt { get; set; }

    public string? Description { get; set; }

    /// <summary>المستخدم المسؤول عن إدارة هذا العقار (Property Manager).</summary>
    public Guid? ManagerUserId { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
    public ICollection<PropertyManagementAgreement> ManagementAgreements { get; set; } = new List<PropertyManagementAgreement>();
}
