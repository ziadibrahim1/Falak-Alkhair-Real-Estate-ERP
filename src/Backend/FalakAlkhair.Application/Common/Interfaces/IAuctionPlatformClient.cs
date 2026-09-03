using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// عقد التكامل مع منصة المزادات العقارية المستقلة لشركة فلك الخير. الـ ERP لا
/// يدمج منطق المزايدة الحية داخله — هذا العقد هو نقطة الاتصال الوحيدة الصادرة
/// (Outbound) نحو المنصة؛ الأحداث الواردة (Bids, Awards ...) تصل عبر Webhook
/// منفصل (راجع AuctionWebhooksController)، وليس عبر هذا العقد.
/// التنفيذ الفعلي (Infrastructure) يتطلب تكوين مزوّد حقيقي (BaseUrl + مفتاح API)؛
/// دون ذلك يرمي BusinessRuleException واضحًا بدل ادّعاء تكامل غير موجود.
/// </summary>
public interface IAuctionPlatformClient
{
    /// <summary>ينشر المزاد على المنصة الخارجية ويعيد معرّفه هناك (ExternalAuctionId).</summary>
    Task<string> PublishAuctionAsync(Auction auction, CancellationToken cancellationToken);

    /// <summary>يطلب إغلاق المزاد على المنصة الخارجية (مثال: إلغاء يدوي من الـ ERP).</summary>
    Task CloseAuctionAsync(string externalAuctionId, CancellationToken cancellationToken);
}
