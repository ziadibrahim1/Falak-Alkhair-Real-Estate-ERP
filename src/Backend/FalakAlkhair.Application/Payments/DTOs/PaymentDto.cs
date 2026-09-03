using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Application.Payments.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = default!;
    public Guid LeaseId { get; set; }
    public string LeaseNumber { get; set; } = default!;
    public Guid TenantId { get; set; }
    public string TenantNameAr { get; set; } = default!;
    public Guid? LeasePaymentId { get; set; }
    public int? InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>سطر في تقرير المتأخرات (Overdue Payments Dashboard — البند 15).</summary>
public class OverduePaymentDto
{
    public Guid LeasePaymentId { get; set; }
    public Guid LeaseId { get; set; }
    public string LeaseNumber { get; set; } = default!;
    public Guid TenantId { get; set; }
    public string TenantNameAr { get; set; } = default!;
    public string TenantMobile { get; set; } = default!;
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int DaysOverdue { get; set; }
}
