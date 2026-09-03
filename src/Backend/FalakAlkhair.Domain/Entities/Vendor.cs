using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>شركة/مورّد صيانة خارجي.</summary>
public class Vendor : BaseAuditableEntity
{
    public string VendorCode { get; set; } = default!; // VEND-000001

    public string NameAr { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? Services { get; set; }
    public decimal? Rating { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MaintenanceRequest> AssignedRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<MaintenanceQuotation> Quotations { get; set; } = new List<MaintenanceQuotation>();
}
