using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// مسوّق عقاري (Real Estate Agent). يحمل رخصة فال (FAL) وفق متطلبات الهيئة
/// العامة للعقار (REGA)، ويشكّل مرجعًا للعملاء المحتملين (Leads) وعمولاته
/// (Commissions) الناتجة عن عقود الإيجار/البيع التي يديرها.
/// </summary>
public class Agent : BaseAuditableEntity
{
    public string AgentCode { get; set; } = default!; // AGENT-000001

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? NationalId { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }

    /// <summary>رقم رخصة فال (FAL) الصادرة من الهيئة العامة للعقار.</summary>
    public string? FalLicenseNumber { get; set; }
    public DateTime? FalLicenseExpiryDate { get; set; }

    public string? Specialization { get; set; }

    /// <summary>المستخدم (من نظام الهوية) المسؤول عن إدارة هذا المسوّق. بلا قيد FK صريح
    /// لأن Domain لا يعتمد على طبقة الهوية (Infrastructure) — يُحقَّق منه عند الاستخدام.</summary>
    public Guid? ManagerUserId { get; set; }

    public AgentStatus Status { get; set; } = AgentStatus.Active;

    public CommissionType CommissionSchemeType { get; set; } = CommissionType.Percentage;
    public decimal DefaultCommissionPercentage { get; set; }
    public decimal? DefaultCommissionFixedAmount { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
