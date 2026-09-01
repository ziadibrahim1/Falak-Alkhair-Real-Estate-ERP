using System.Security.Claims;
using FalakAlkhair.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FalakAlkhair.Infrastructure.Services;

/// <summary>يستخرج بيانات المستخدم الحالي من الـ Claims الموجودة داخل JWT.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name) ?? User?.Identity?.Name;

    public Guid? CompanyId
    {
        get
        {
            var value = User?.FindFirstValue("company_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? BranchId
    {
        get
        {
            var value = User?.FindFirstValue("branch_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToList() ?? new List<string>();

    public bool HasPermission(string permissionCode) =>
        User?.HasClaim("permission", permissionCode) ?? false;
}
