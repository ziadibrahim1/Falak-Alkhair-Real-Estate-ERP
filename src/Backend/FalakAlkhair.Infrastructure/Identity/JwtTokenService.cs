using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FalakAlkhair.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FalakAlkhair.Infrastructure.Identity;

/// <summary>يصدر Access Token (JWT قصير العمر) و Refresh Token (طويل العمر، عشوائي، يُخزَّن مُجزَّأً بحكم الطابع الفريد).</summary>
public class JwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly ApplicationDbContext _context;

    public JwtTokenService(IOptions<JwtSettings> settings, ApplicationDbContext context)
    {
        _settings = settings.Value;
        _context = context;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissionCodes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("full_name", user.FullNameAr),
            new("company_id", user.CompanyId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("branch_id", user.BranchId.Value.ToString()));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissionCodes.Distinct().Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public async Task<Domain.Entities.RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        var refreshToken = new Domain.Entities.RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public Task<Domain.Entities.RefreshToken?> FindActiveRefreshTokenAsync(string token, CancellationToken cancellationToken) =>
        _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
}
