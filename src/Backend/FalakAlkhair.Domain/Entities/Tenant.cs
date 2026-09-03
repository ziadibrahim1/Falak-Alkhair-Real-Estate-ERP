using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>مستأجر (فرد أو شركة). يشكّل أساس CRM المستأجرين ومرجعًا لعقود الإيجار.</summary>
public class Tenant : BaseAuditableEntity
{
    public string TenantCode { get; set; } = default!; // TEN-000001

    public PartyType PartyType { get; set; } = PartyType.Individual;

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    public string? NationalId { get; set; }
    public string? CommercialRegistrationNumber { get; set; }

    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }

    public string? NationalAddress { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    public string? Employer { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
