namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>نوع طلب الصيانة.</summary>
public enum MaintenanceRequestType
{
    Electrical = 1,
    Plumbing = 2,
    AC = 3,
    Structural = 4,
    Appliance = 5,
    Cleaning = 6,
    Other = 99
}

/// <summary>أولوية طلب الصيانة.</summary>
public enum MaintenancePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>حالة طلب الصيانة ضمن دورة العمل الكاملة.</summary>
public enum MaintenanceStatus
{
    New = 1,
    Assigned = 2,
    Inspection = 3,
    Quotation = 4,
    WaitingApproval = 5,
    Approved = 6,
    InProgress = 7,
    WaitingParts = 8,
    Completed = 9,
    Cancelled = 10
}

/// <summary>حالة عرض سعر الصيانة المقدَّم من مورّد.</summary>
public enum QuotationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
