using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// سجل تدقيق مزاد غير قابل للتعديل (Append-Only) — لا يوجد أي أمر Update/Delete
/// له في طبقة Application عمدًا، تحقيقًا لمتطلب "لا تسمح بتعديل سجل المزايدات
/// بعد تسجيله". يُنشَأ إما من إجراءات داخلية (اعتماد، نشر، إلغاء ...) أو من
/// أحداث فعلية واردة من منصة المزادات المستقلة عبر Webhook.
/// </summary>
public class AuctionAuditLog : BaseAuditableEntity
{
    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; } = default!;

    public AuctionEventType EventType { get; set; }

    /// <summary>الحمولة الخام (JSON) كما وردت من المنصة الخارجية، للتتبّع الكامل.</summary>
    public string? Payload { get; set; }

    public string? SourceIp { get; set; }
    public string? Notes { get; set; }

    public DateTime OccurredAt { get; set; }
}
