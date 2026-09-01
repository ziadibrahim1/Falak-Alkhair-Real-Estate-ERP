using Microsoft.AspNetCore.Identity;

namespace FalakAlkhair.Infrastructure.Identity;

/// <summary>
/// دور مستخدم. الأدوار الأساسية (Super Admin, System Administrator ...) تُزرع
/// كبيانات Seed بعلامة IsSystemRole=true، لكن Administrator يستطيع إنشاء أدوار
/// إضافية غير محمية من واجهة الإدارة عبر IRoleManagementService.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string NameAr { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    /// <summary>null تعني دورًا عامًا (Global) غير مرتبط بشركة معيّنة.</summary>
    public Guid? CompanyId { get; set; }
}
