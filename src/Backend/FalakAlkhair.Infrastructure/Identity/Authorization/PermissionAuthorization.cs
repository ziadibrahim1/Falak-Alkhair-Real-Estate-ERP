using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace FalakAlkhair.Infrastructure.Identity.Authorization;

/// <summary>متطلب صلاحية واحدة، مثال: "Property.View".</summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

/// <summary>يتحقق من وجود Claim من نوع "permission" بقيمة تطابق الصلاحية المطلوبة، أو أن المستخدم SuperAdmin.</summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.IsInRole(Common.Constants.SystemRoles.SuperAdmin) ||
            context.User.HasClaim("permission", requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// يبني سياسات Authorization ديناميكيًا لأي اسم يبدأ بـ "Permission:"، بدل تسجيل
/// سياسة يدوية ثابتة لكل صلاحية من صلاحيات النظام. هذا ما يسمح بإضافة صلاحيات
/// جديدة (حتى المُنشأة ديناميكيًا من واجهة الإدارة) دون تعديل كود الـ Startup.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string Prefix = "Permission:";
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[Prefix.Length..];
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
