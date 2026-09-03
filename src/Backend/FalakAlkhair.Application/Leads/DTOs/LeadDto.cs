using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Leads.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string LeadCode { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public LeadSource Source { get; set; }
    public LeadType LeadType { get; set; }
    public Guid? InterestedPropertyId { get; set; }
    public string? InterestedPropertyName { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentNameAr { get; set; }
    public LeadStatus Status { get; set; }
    public LeadPriority Priority { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static LeadDto FromEntity(Lead lead) => new()
    {
        Id = lead.Id,
        LeadCode = lead.LeadCode,
        NameAr = lead.NameAr,
        Mobile = lead.Mobile,
        Email = lead.Email,
        Source = lead.Source,
        LeadType = lead.LeadType,
        InterestedPropertyId = lead.InterestedPropertyId,
        InterestedPropertyName = lead.InterestedProperty?.PropertyName,
        AssignedAgentId = lead.AssignedAgentId,
        AssignedAgentNameAr = lead.AssignedAgent?.NameAr,
        Status = lead.Status,
        Priority = lead.Priority,
        Notes = lead.Notes,
        CreatedAt = lead.CreatedAt
    };
}
