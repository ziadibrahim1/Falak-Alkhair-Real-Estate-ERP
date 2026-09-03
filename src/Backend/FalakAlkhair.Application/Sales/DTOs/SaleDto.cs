using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Sales.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public Guid SellerId { get; set; }
    public string SellerCode { get; set; } = default!;
    public Guid BuyerId { get; set; }
    public string BuyerNameAr { get; set; } = default!;
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public decimal AskingPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal CommissionPercentage { get; set; }
    public SaleStage Stage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static SaleDto FromEntity(Sale sale) => new()
    {
        Id = sale.Id,
        SaleNumber = sale.SaleNumber,
        PropertyId = sale.PropertyId,
        PropertyName = sale.Property?.PropertyName ?? string.Empty,
        UnitId = sale.UnitId,
        UnitNumber = sale.Unit?.UnitNumber ?? string.Empty,
        SellerId = sale.SellerId,
        SellerCode = sale.Seller?.SellerCode ?? string.Empty,
        BuyerId = sale.BuyerId,
        BuyerNameAr = sale.Buyer?.NameAr ?? string.Empty,
        AgentId = sale.AgentId,
        AgentNameAr = sale.Agent?.NameAr,
        AskingPrice = sale.AskingPrice,
        FinalPrice = sale.FinalPrice,
        CommissionPercentage = sale.CommissionPercentage,
        Stage = sale.Stage,
        CompletedAt = sale.CompletedAt,
        Notes = sale.Notes,
        CreatedAt = sale.CreatedAt
    };
}
