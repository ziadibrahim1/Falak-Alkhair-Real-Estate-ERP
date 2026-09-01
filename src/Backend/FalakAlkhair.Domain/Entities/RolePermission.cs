using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// ربط دور (ApplicationRole في طبقة Infrastructure/Identity) بصلاحية معيّنة.
/// أُبقي الربط بمعرّف الدور (RoleId) فقط هنا حتى لا تعتمد طبقة Domain على
/// ASP.NET Core Identity.
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
}
