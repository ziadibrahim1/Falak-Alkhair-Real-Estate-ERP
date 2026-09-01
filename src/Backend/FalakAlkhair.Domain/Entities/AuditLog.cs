using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// سجل تدقيق غير قابل للتعديل (Append-Only). يُكتب تلقائيًا من
/// AuditableEntitySaveChangesInterceptor في طبقة Infrastructure لأي تغيير على
/// كيان يرث BaseAuditableEntity، بالإضافة لأحداث يدوية مثل تسجيل الدخول.
/// لا يوجد Id من BaseEntity هنا عمدًا: هذا السجل لا يُعدَّل ولا يُحذف أبدًا.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }
    public string? UserName { get; set; }

    public string EntityType { get; set; } = default!;
    public string? EntityId { get; set; }

    public AuditAction Action { get; set; }

    /// <summary>القيم القديمة بصيغة JSON قبل التعديل.</summary>
    public string? OldValues { get; set; }

    /// <summary>القيم الجديدة بصيغة JSON بعد التعديل.</summary>
    public string? NewValues { get; set; }

    /// <summary>أسماء الحقول التي تغيّرت فعليًا.</summary>
    public string? AffectedColumns { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
