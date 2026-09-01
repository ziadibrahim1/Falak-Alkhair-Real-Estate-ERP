using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// الشركة المالكة للنظام (Multi-Company Ready).
/// حاليًا شركة واحدة (فلك الخير العقارية) لكن البنية تدعم أكثر من شركة مستقبلًا.
/// </summary>
public class Company : BaseEntity
{
    public string Code { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? FalLicenseNumber { get; set; }      // رخصة فال (وساطة عقارية)
    public DateTime? FalLicenseExpiryDate { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
