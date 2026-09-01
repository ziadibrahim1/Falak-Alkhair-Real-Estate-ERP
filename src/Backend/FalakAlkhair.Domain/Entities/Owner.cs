using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// مالك عقار (فرد أو شركة). يشكّل أساس CRM الملاك، ومرجعًا لعقود إدارة
/// الأملاك والعقارات المملوكة له.
/// </summary>
public class Owner : BaseAuditableEntity
{
    public string OwnerCode { get; set; } = default!; // OWNER-000001

    public PartyType PartyType { get; set; } = PartyType.Individual;

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    /// <summary>رقم الهوية الوطنية / الإقامة (للأفراد).</summary>
    public string? NationalId { get; set; }

    /// <summary>السجل التجاري (للشركات).</summary>
    public string? CommercialRegistrationNumber { get; set; }

    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }

    public string? NationalAddress { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    public string? BankName { get; set; }
    public string? Iban { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<PropertyManagementAgreement> ManagementAgreements { get; set; } = new List<PropertyManagementAgreement>();
}
