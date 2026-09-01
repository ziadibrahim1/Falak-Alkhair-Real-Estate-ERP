using FalakAlkhair.Application.Common.Interfaces;

namespace FalakAlkhair.UnitTests.TestHelpers;

/// <summary>مستخدم وهمي ثابت لأغراض الاختبار، بدل الاعتماد على HttpContext الحقيقي.</summary>
public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public string? UserName { get; set; } = "test.user";
    public Guid? CompanyId { get; set; } = Guid.NewGuid();
    public Guid? BranchId { get; set; } = Guid.NewGuid();
    public string? IpAddress { get; set; } = "127.0.0.1";
    public string? UserAgent { get; set; } = "xunit-test-agent";
    public bool IsAuthenticated { get; set; } = true;
    public IReadOnlyList<string> Permissions { get; set; } = new List<string>();

    public bool HasPermission(string permissionCode) => Permissions.Contains(permissionCode);
}
