using Microsoft.AspNetCore.Identity;

namespace FalakAlkhair.Infrastructure.Identity;

/// <summary>
/// مستخدم النظام مبني فوق ASP.NET Core Identity، مع الحقول الإضافية اللازمة
/// لتعدد الشركات/الفروع وبيانات الموظف.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullNameAr { get; set; } = default!;
    public string? FullNameEn { get; set; }
    public string? EmployeeNumber { get; set; }

    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
