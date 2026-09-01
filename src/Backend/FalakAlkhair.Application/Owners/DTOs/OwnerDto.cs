using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Owners.DTOs;

public class OwnerDto
{
    public Guid Id { get; set; }
    public string OwnerCode { get; set; } = default!;
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
    public string? BankName { get; set; }
    public string? Iban { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int PropertiesCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static OwnerDto FromEntity(Owner owner) => new()
    {
        Id = owner.Id,
        OwnerCode = owner.OwnerCode,
        PartyType = owner.PartyType,
        NameAr = owner.NameAr,
        NameEn = owner.NameEn,
        NationalId = owner.NationalId,
        CommercialRegistrationNumber = owner.CommercialRegistrationNumber,
        Mobile = owner.Mobile,
        Email = owner.Email,
        NationalAddress = owner.NationalAddress,
        City = owner.City,
        District = owner.District,
        BankName = owner.BankName,
        Iban = owner.Iban,
        Notes = owner.Notes,
        IsActive = owner.IsActive,
        PropertiesCount = owner.Properties?.Count ?? 0,
        CreatedAt = owner.CreatedAt
    };
}
