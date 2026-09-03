using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// إشعار داخل النظام. UserId اختياري: عند تحديده يكون الإشعار موجَّهًا لمستخدم
/// بعينه (مثال: صاحب صلاحية الاعتماد الذي أنشأ الإجراء)، وعند تركه فارغًا يكون
/// إشعارًا عامًا مرئيًا لكل مستخدمي الشركة (Company-wide Broadcast) — مثال:
/// تنبيه صيانة عاجلة يهمّ أي مستخدم بصلاحية العرض، لا مستخدمًا بعينه.
/// </summary>
public class Notification : BaseAuditableEntity
{
    public Guid? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;

    /// <summary>رابط نسبي في الواجهة الأمامية للانتقال إليه عند الضغط على الإشعار، مثال: "/leads/{id}".</summary>
    public string? Link { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
