namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>
/// حالة عقد الإيجار. القيم "Expiring"/"Expired" لا تُخزَّن كحالة صريحة — بل تُحسب
/// وقت الاستعلام من EndDate مقابل الحالة Active (بنفس أسلوب DaysRemaining في
/// عقود إدارة الأملاك) تجنبًا للحاجة إلى Background Job لتحديث الحالة دوريًا.
/// </summary>
public enum LeaseStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Terminated = 4,
    Cancelled = 5
}

/// <summary>دورية سداد الإيجار.</summary>
public enum PaymentFrequency
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Annual = 4
}

/// <summary>حالة دفعة مجدولة ضمن جدول سداد عقد الإيجار.</summary>
public enum LeasePaymentStatus
{
    Pending = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 4
}

/// <summary>طريقة سداد الدفعة الفعلية.</summary>
public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Pos = 3,
    Online = 4,
    Cheque = 5,
    Other = 99
}
