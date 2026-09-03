using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Tenants.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string TenantCode { get; set; } = default!;
    public PartyType PartyType { get; set; }
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
    public bool IsActive { get; set; }
    public int LeasesCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static TenantDto FromEntity(Tenant tenant) => new()
    {
        Id = tenant.Id,
        TenantCode = tenant.TenantCode,
        PartyType = tenant.PartyType,
        NameAr = tenant.NameAr,
        NameEn = tenant.NameEn,
        NationalId = tenant.NationalId,
        CommercialRegistrationNumber = tenant.CommercialRegistrationNumber,
        Mobile = tenant.Mobile,
        Email = tenant.Email,
        NationalAddress = tenant.NationalAddress,
        City = tenant.City,
        District = tenant.District,
        Employer = tenant.Employer,
        Notes = tenant.Notes,
        IsActive = tenant.IsActive,
        LeasesCount = tenant.Leases?.Count ?? 0,
        CreatedAt = tenant.CreatedAt
    };
}
