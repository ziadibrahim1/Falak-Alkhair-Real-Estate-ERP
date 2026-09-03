using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// يضيف صف Notification إلى الـ DbContext الحالي (دون استدعاء SaveChanges بنفسه)
/// حتى يُحفَظ ضمن نفس معاملة SaveChangesAsync الخاصة بالـ Handler المستدعي —
/// بنفس فلسفة إضافة AuctionAuditLog داخل معالجات المزادات.
/// </summary>
public interface INotificationService
{
    void Notify(Guid companyId, Guid? branchId, Guid? userId, NotificationType type, string title, string message, string? link = null);
}
