namespace FalakAlkhair.Application.Common.Interfaces;

public record RoleDto(Guid Id, string Name, string NameAr, string? Description, bool IsSystemRole, IReadOnlyList<string> PermissionCodes);

/// <summary>
/// إدارة الأدوار وربطها بالصلاحيات. تسمح لمدير النظام بإنشاء أدوار جديدة
/// وتحديد صلاحياتها ديناميكيًا (بدل الاكتفاء بالأدوار الثابتة المزروعة Seed).
/// </summary>
public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);

    Task<Guid> CreateRoleAsync(string name, string nameAr, string? description, IEnumerable<string> permissionCodes, CancellationToken cancellationToken);

    Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken);
}
