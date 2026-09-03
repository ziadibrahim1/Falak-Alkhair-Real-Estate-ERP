using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Leases.DTOs;

public class LeaseDto
{
    public Guid Id { get; set; }
    public string LeaseNumber { get; set; } = default!;
    public Guid TenantId { get; set; }
    public string TenantNameAr { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AnnualRentAmount { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; }
    public int NumberOfPayments { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal VatPercentage { get; set; }
    public LeaseStatus Status { get; set; }
    public int DaysRemaining { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LeasePaymentDto> Payments { get; set; } = new();
}

public class LeasePaymentDto
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public LeasePaymentStatus Status { get; set; }
    public bool IsOverdue { get; set; }

    public static LeasePaymentDto FromEntity(LeasePayment p) => new()
    {
        Id = p.Id,
        InstallmentNumber = p.InstallmentNumber,
        DueDate = p.DueDate,
        Amount = p.Amount,
        PaidAmount = p.PaidAmount,
        RemainingAmount = p.Amount - p.PaidAmount,
        Status = p.Status,
        IsOverdue = p.Status != LeasePaymentStatus.Paid && p.Status != LeasePaymentStatus.Cancelled && p.DueDate.Date < DateTime.UtcNow.Date
    };
}
