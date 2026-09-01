using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Agreements.DTOs;

public class AgreementDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ManagementFee { get; set; }
    public CommissionType CommissionType { get; set; }
    public decimal CommissionPercentage { get; set; }
    public ManagementAgreementStatus Status { get; set; }
    public int DaysRemaining { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AgreementDto FromEntity(PropertyManagementAgreement a) => new()
    {
        Id = a.Id,
        ContractNumber = a.ContractNumber,
        OwnerId = a.OwnerId,
        OwnerNameAr = a.Owner?.NameAr ?? string.Empty,
        PropertyId = a.PropertyId,
        PropertyName = a.Property?.PropertyName ?? string.Empty,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        ManagementFee = a.ManagementFee,
        CommissionType = a.CommissionType,
        CommissionPercentage = a.CommissionPercentage,
        Status = a.Status,
        DaysRemaining = (a.EndDate.Date - DateTime.UtcNow.Date).Days,
        CreatedAt = a.CreatedAt
    };
}
