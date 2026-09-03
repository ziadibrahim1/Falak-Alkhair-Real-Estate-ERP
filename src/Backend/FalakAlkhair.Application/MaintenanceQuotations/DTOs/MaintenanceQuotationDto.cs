using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.MaintenanceQuotations.DTOs;

public class MaintenanceQuotationItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public static MaintenanceQuotationItemDto FromEntity(MaintenanceQuotationItem item) => new()
    {
        Id = item.Id,
        Description = item.Description,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        LineTotal = item.LineTotal
    };
}

public class MaintenanceQuotationDto
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = default!;
    public Guid VendorId { get; set; }
    public string VendorNameAr { get; set; } = default!;
    public Guid MaintenanceRequestId { get; set; }
    public string MaintenanceRequestNumber { get; set; } = default!;
    public DateTime? ValidUntil { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public QuotationStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<MaintenanceQuotationItemDto> Items { get; set; } = new();

    public static MaintenanceQuotationDto FromEntity(MaintenanceQuotation quotation) => new()
    {
        Id = quotation.Id,
        QuotationNumber = quotation.QuotationNumber,
        VendorId = quotation.VendorId,
        VendorNameAr = quotation.Vendor?.NameAr ?? string.Empty,
        MaintenanceRequestId = quotation.MaintenanceRequestId,
        MaintenanceRequestNumber = quotation.MaintenanceRequest?.RequestNumber ?? string.Empty,
        ValidUntil = quotation.ValidUntil,
        VatPercentage = quotation.VatPercentage,
        SubtotalAmount = quotation.SubtotalAmount,
        VatAmount = quotation.VatAmount,
        TotalAmount = quotation.TotalAmount,
        Status = quotation.Status,
        Notes = quotation.Notes,
        Items = quotation.Items?.Select(MaintenanceQuotationItemDto.FromEntity).ToList() ?? new()
    };
}
