using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// بيانات المزاد الأساسية (Master Data) كما يحتفظ بها الـ ERP. لا تُخزَّن هنا
/// المزايدات الحية أو سجل المزايدين — تلك تعيش على منصة المزادات المستقلة
/// وتصل عبر Webhooks (راجع AuctionAuditLog وAuctionWebhooksController).
/// </summary>
public class Auction : BaseAuditableEntity
{
    public string AuctionNumber { get; set; } = default!; // AUCT-000001

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = default!;

    public Guid? SellerId { get; set; }
    public Seller? Seller { get; set; }

    /// <summary>المسوّق/مدير المزاد المسؤول (اختياري).</summary>
    public Guid? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? DepositAmount { get; set; }

    public decimal CommissionPercentage { get; set; }
    public decimal VatPercentage { get; set; } = 15;

    public AuctionStatus Status { get; set; } = AuctionStatus.Draft;

    public Guid? WinnerBuyerId { get; set; }
    public Buyer? WinnerBuyer { get; set; }
    public decimal? FinalPrice { get; set; }

    /// <summary>معرّف المزاد على المنصة الخارجية بعد النشر (Null إن لم تُفعَّل التكامل بعد).</summary>
    public string? ExternalAuctionId { get; set; }
    public string? ExternalPlatformUrl { get; set; }

    /// <summary>آخر مبلغ مزايدة معروف من المنصة الخارجية — معلوماتي فقط، وليس مصدر الحقيقة للمزايدة.</summary>
    public decimal? CurrentBidAmount { get; set; }
    public int BidsCount { get; set; }

    public string? CancellationReason { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<AuctionAuditLog> AuditLogs { get; set; } = new List<AuctionAuditLog>();
    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
