using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// مشترٍ محتمل (Buyer CRM). يحمل معايير البحث (الميزانية، المدينة، المساحة ...)
/// المستخدمة في محرك المطابقة البسيط (Buyer-Property Matching).
/// </summary>
public class Buyer : BaseAuditableEntity
{
    public string BuyerCode { get; set; } = default!; // BUYER-000001

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? NationalId { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }

    public decimal? Budget { get; set; }
    public string? PreferredCity { get; set; }
    public string? PreferredDistrict { get; set; }
    public PropertyType? PreferredPropertyType { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }

    public BuyerPurpose Purpose { get; set; } = BuyerPurpose.PersonalUse;
    public FinancingStatus FinancingStatus { get; set; } = FinancingStatus.Undetermined;

    public Guid? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
