using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عميل محتمل (Lead) — نقطة الدخول المركزية لـ CRM النظام قبل التحوّل إلى
/// مشترٍ/مستأجر/مالك/بائع/مستثمر/مورّد فعلي.
/// </summary>
public class Lead : BaseAuditableEntity
{
    public string LeadCode { get; set; } = default!; // LEAD-000001

    public string NameAr { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }

    public LeadSource Source { get; set; } = LeadSource.Other;
    public LeadType LeadType { get; set; }

    public Guid? InterestedPropertyId { get; set; }
    public Property? InterestedProperty { get; set; }

    public Guid? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;

    public string? Notes { get; set; }
}
