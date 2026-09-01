namespace FalakAlkhair.Infrastructure.Identity;

/// <summary>إعدادات JWT — تُقرأ من appsettings/متغيرات البيئة، ولا تحتوي أي قيمة سرّية داخل الكود.</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int AccessTokenExpiryMinutes { get; set; } = 30;
    public int RefreshTokenExpiryDays { get; set; } = 14;
}
