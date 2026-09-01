namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>نوع صاحب السجل: فرد أو شركة/جهة اعتبارية.</summary>
public enum PartyType
{
    Individual = 1,
    Company = 2
}

/// <summary>نوع الإجراء المسجّل في سجل التدقيق (Audit Log).</summary>
public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Approve = 4,
    Reject = 5,
    Login = 6,
    Logout = 7,
    Export = 8,
    Print = 9,
    PermissionChange = 10,
    StatusChange = 11
}

/// <summary>حالة الموافقة العامة المستخدمة في أكثر من Workflow.</summary>
public enum ApprovalStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4
}
