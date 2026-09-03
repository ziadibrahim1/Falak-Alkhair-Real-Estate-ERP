using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Vendors.DTOs;

public class VendorDto
{
    public Guid Id { get; set; }
    public string VendorCode { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? Services { get; set; }
    public decimal? Rating { get; set; }
    public bool IsActive { get; set; }
    public int AssignedRequestsCount { get; set; }

    public static VendorDto FromEntity(Vendor vendor) => new()
    {
        Id = vendor.Id,
        VendorCode = vendor.VendorCode,
        NameAr = vendor.NameAr,
        ContactPerson = vendor.ContactPerson,
        Mobile = vendor.Mobile,
        Email = vendor.Email,
        CommercialRegistrationNumber = vendor.CommercialRegistrationNumber,
        VatNumber = vendor.VatNumber,
        Services = vendor.Services,
        Rating = vendor.Rating,
        IsActive = vendor.IsActive,
        AssignedRequestsCount = vendor.AssignedRequests?.Count(r => !r.IsDeleted) ?? 0
    };
}
