namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// معلومات المستخدم الحالي المستخرجة من JWT، تُستخدم لتطبيق Multi-Company /
/// Multi-Branch scoping ولتعبئة CreatedBy/UpdatedBy وسجلات التدقيق.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    Guid? CompanyId { get; }
    Guid? BranchId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permissionCode);
}
