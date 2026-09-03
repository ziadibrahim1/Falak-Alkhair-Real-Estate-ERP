using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// معاملة سداد فعلية (تحصيل) تُسجَّل مقابل دفعة مجدولة (LeasePayment) ضمن
/// عقد إيجار. جزء من وحدة التحصيل والمدفوعات (Accounts Receivable).
/// </summary>
public class Payment : BaseAuditableEntity
{
    public string PaymentNumber { get; set; } = default!; // PAY-000001

    public Guid LeaseId { get; set; }
    public Lease Lease { get; set; } = default!;

    public Guid? LeasePaymentId { get; set; }
    public LeasePayment? LeasePayment { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
}
