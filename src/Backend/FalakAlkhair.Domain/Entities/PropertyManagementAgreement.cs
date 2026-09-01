using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عقد إدارة أملاك بين الشركة ومالك العقار. يحكمه Workflow:
/// Draft → PendingApproval → Active → Expiring → Expired → Terminated.
/// </summary>
public class PropertyManagementAgreement : BaseAuditableEntity
{
    public string ContractNumber { get; set; } = default!; // PMA-000001

    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = default!;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal ManagementFee { get; set; }
    public CommissionType CommissionType { get; set; }
    public decimal CommissionPercentage { get; set; }

    public string? PaymentTerms { get; set; }
    public string? Responsibilities { get; set; }
    public string? RenewalTerms { get; set; }
    public string? TerminationTerms { get; set; }

    public ManagementAgreementStatus Status { get; set; } = ManagementAgreementStatus.Draft;

    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
}
