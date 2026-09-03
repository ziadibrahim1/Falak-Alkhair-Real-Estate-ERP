using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// دفعة مجدولة (قسط) ضمن جدول سداد عقد إيجار. يُولَّد تلقائيًا عند إنشاء
/// العقد حسب دورية السداد (شهري/ربعي/نصف سنوي/سنوي)، وتُحدَّث حالته وقيمته
/// المسددة عند تسجيل دفعات فعلية (Payment) عليه.
/// </summary>
public class LeasePayment : BaseAuditableEntity
{
    public Guid LeaseId { get; set; }
    public Lease Lease { get; set; } = default!;

    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public LeasePaymentStatus Status { get; set; } = LeasePaymentStatus.Pending;

    public ICollection<Payment> PaymentTransactions { get; set; } = new List<Payment>();
}
