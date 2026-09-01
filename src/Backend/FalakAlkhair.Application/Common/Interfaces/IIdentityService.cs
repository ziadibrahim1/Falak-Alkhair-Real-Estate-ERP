namespace FalakAlkhair.Application.Common.Interfaces;

public record AuthResult(
    bool Succeeded,
    string? UserId,
    string? AccessToken,
    string? RefreshToken,
    DateTime? AccessTokenExpiresAt,
    IEnumerable<string> Errors);

/// <summary>
/// عقد خدمات الهوية (تسجيل الدخول، إصدار/تدوير التوكنات، إدارة المستخدمين)
/// المُنفَّذ في طبقة Infrastructure فوق ASP.NET Core Identity + JWT.
/// </summary>
public interface IIdentityService
{
    Task<AuthResult> LoginAsync(string userNameOrEmail, string password, string? ipAddress, CancellationToken cancellationToken);

    Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken);

    Task RevokeTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken);

    Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string userName,
        string email,
        string password,
        string fullNameAr,
        Guid companyId,
        Guid? branchId,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken);
}
