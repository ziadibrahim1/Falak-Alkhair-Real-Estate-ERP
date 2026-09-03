namespace FalakAlkhair.Infrastructure.Integrations.Auctions;

/// <summary>
/// إعدادات الاتصال بمنصة المزادات المستقلة. تبقى فارغة افتراضيًا حتى تُربَط
/// شركة فلك الخير فعليًا بمزوّد حقيقي — لا قيمة افتراضية وهمية.
/// </summary>
public class AuctionPlatformSettings
{
    public const string SectionName = "AuctionIntegration";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }

    /// <summary>السرّ المشترك للتحقق من صحة طلبات Webhook الواردة من المنصة الخارجية.</summary>
    public string? WebhookSecret { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(ApiKey);
}
