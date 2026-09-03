namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>
/// حالة المزاد ضمن دورة عمل كاملة. الـ ERP يحتفظ فقط ببيانات المزاد الأساسية
/// (Master Data) والتسوية المالية؛ المزايدة الحية الفعلية تجري على منصة
/// المزادات المستقلة (راجع IAuctionPlatformClient وAuctionWebhooksController).
/// </summary>
public enum AuctionStatus
{
    Draft = 1,
    PendingApproval = 2,
    Scheduled = 3,
    Published = 4,
    Live = 5,
    Ended = 6,
    Awarded = 7,
    Cancelled = 8,
    Settled = 9
}

/// <summary>نوع الحدث المسجَّل في سجل تدقيق المزاد (Auction Audit) — غير قابل للتعديل بعد التسجيل.</summary>
public enum AuctionEventType
{
    AuctionCreated = 1,
    AuctionApproved = 2,
    AuctionPublished = 3,
    AuctionWentLive = 4,
    BidPlaced = 5,
    AuctionExtended = 6,
    AuctionEnded = 7,
    AuctionAwarded = 8,
    AuctionSettled = 9,
    AuctionCancelled = 10,
    PaymentReceived = 11
}
