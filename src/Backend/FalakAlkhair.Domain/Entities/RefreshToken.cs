using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>Refresh Token لتدوير رموز JWT بأمان دون إعادة تسجيل الدخول المتكرر.</summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
