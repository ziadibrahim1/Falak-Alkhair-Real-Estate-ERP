using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Agents.DTOs;

public class AgentDto
{
    public Guid Id { get; set; }
    public string AgentCode { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? NationalId { get; set; }
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public string? FalLicenseNumber { get; set; }
    public DateTime? FalLicenseExpiryDate { get; set; }
    public string? Specialization { get; set; }
    public Guid? ManagerUserId { get; set; }
    public AgentStatus Status { get; set; }
    public CommissionType CommissionSchemeType { get; set; }
    public decimal DefaultCommissionPercentage { get; set; }
    public decimal? DefaultCommissionFixedAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int CommissionsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AgentDto FromEntity(Agent agent) => new()
    {
        Id = agent.Id,
        AgentCode = agent.AgentCode,
        NameAr = agent.NameAr,
        NameEn = agent.NameEn,
        NationalId = agent.NationalId,
        Mobile = agent.Mobile,
        Email = agent.Email,
        FalLicenseNumber = agent.FalLicenseNumber,
        FalLicenseExpiryDate = agent.FalLicenseExpiryDate,
        Specialization = agent.Specialization,
        ManagerUserId = agent.ManagerUserId,
        Status = agent.Status,
        CommissionSchemeType = agent.CommissionSchemeType,
        DefaultCommissionPercentage = agent.DefaultCommissionPercentage,
        DefaultCommissionFixedAmount = agent.DefaultCommissionFixedAmount,
        Notes = agent.Notes,
        IsActive = agent.IsActive,
        CommissionsCount = agent.Commissions?.Count ?? 0,
        CreatedAt = agent.CreatedAt
    };
}
