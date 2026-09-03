using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Commissions.DTOs;

public class CommissionDto
{
    public Guid Id { get; set; }
    public string CommissionNumber { get; set; } = default!;
    public Guid AgentId { get; set; }
    public string AgentNameAr { get; set; } = default!;
    public CommissionSourceType SourceType { get; set; }
    public Guid? LeaseId { get; set; }
    public string? LeaseNumber { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal NetCommissionAmount { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CommissionDto FromEntity(Commission commission) => new()
    {
        Id = commission.Id,
        CommissionNumber = commission.CommissionNumber,
        AgentId = commission.AgentId,
        AgentNameAr = commission.Agent?.NameAr ?? string.Empty,
        SourceType = commission.SourceType,
        LeaseId = commission.LeaseId,
        LeaseNumber = commission.Lease?.LeaseNumber,
        BaseAmount = commission.BaseAmount,
        CommissionPercentage = commission.CommissionPercentage,
        CommissionAmount = commission.CommissionAmount,
        VatPercentage = commission.VatPercentage,
        VatAmount = commission.VatAmount,
        NetCommissionAmount = commission.NetCommissionAmount,
        Status = commission.Status,
        PaidAt = commission.PaidAt,
        Notes = commission.Notes,
        CreatedAt = commission.CreatedAt
    };
}
