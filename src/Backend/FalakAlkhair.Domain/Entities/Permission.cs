using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// صلاحية ذرّية داخل النظام، مثال: Property.View، Lease.Approve.
/// الصلاحيات مصدرها الكود (Seed) وهي ثابتة على مستوى الميزات، بينما الأدوار
/// (Roles) وربطها بالصلاحيات ديناميكي بالكامل ويُدار من واجهة الإدارة.
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>الكود الفريد، مثال: "Property.View".</summary>
    public string Code { get; set; } = default!;

    /// <summary>اسم الوحدة/الموديول، مثال: "Property".</summary>
    public string Module { get; set; } = default!;

    /// <summary>الإجراء، مثال: View, Create, Edit, Delete, Approve, Reject, Export, Print, Manage, Financial, Assign.</summary>
    public string Action { get; set; } = default!;

    public string DescriptionAr { get; set; } = default!;
    public string? DescriptionEn { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
