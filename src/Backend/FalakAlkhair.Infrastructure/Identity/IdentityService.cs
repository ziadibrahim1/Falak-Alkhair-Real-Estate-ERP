using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtTokenService _tokenService;
    private readonly ApplicationDbContext _context;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        JwtTokenService tokenService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthResult> LoginAsync(string userNameOrEmail, string password, string? ipAddress, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(userNameOrEmail)
                   ?? await _userManager.FindByEmailAsync(userNameOrEmail);

        if (user is null || !user.IsActive)
        {
            return new AuthResult(false, null, null, null, null, new[] { "بيانات الدخول غير صحيحة." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return new AuthResult(false, null, null, null, null, new[] { "الحساب موقوف مؤقتًا بسبب محاولات دخول فاشلة متكررة." });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return new AuthResult(false, null, null, null, null, new[] { "بيانات الدخول غير صحيحة." });
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var existingToken = await _tokenService.FindActiveRefreshTokenAsync(refreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            return new AuthResult(false, null, null, null, null, new[] { "رمز التحديث غير صالح." });
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return new AuthResult(false, null, null, null, null, new[] { "المستخدم غير موجود أو غير نشط." });
        }

        // تدوير التوكن: إلغاء القديم وإصدار جديد لمنع إعادة استخدامه (Refresh Token Rotation).
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;

        var result = await IssueTokensAsync(user, ipAddress, cancellationToken);

        existingToken.ReplacedByToken = result.RefreshToken;
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var existingToken = await _tokenService.FindActiveRefreshTokenAsync(refreshToken, cancellationToken);
        if (existingToken is { IsActive: true })
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string userName, string email, string password, string fullNameAr,
        Guid companyId, Guid? branchId, IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullNameAr = fullNameAr,
            CompanyId = companyId,
            BranchId = branchId,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (false, null, createResult.Errors.Select(e => e.Description));
        }

        var validRoles = roleNames.Where(r => _roleManager.Roles.Any(role => role.Name == r)).ToList();
        if (validRoles.Count != 0)
        {
            await _userManager.AddToRolesAsync(user, validRoles);
        }

        return (true, user.Id.ToString(), Array.Empty<string>());
    }

    private async Task<AuthResult> IssueTokensAsync(ApplicationUser user, string? ipAddress, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var roleIds = await _roleManager.Roles.Where(r => roles.Contains(r.Name!)).Select(r => r.Id).ToListAsync(cancellationToken);
        var permissionCodes = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles, permissionCodes);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, ipAddress, cancellationToken);

        return new AuthResult(true, user.Id.ToString(), accessToken, refreshToken.Token, expiresAt, Array.Empty<string>());
    }
}
