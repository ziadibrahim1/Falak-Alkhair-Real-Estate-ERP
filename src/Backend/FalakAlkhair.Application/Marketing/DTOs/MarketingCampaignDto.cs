using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Marketing.DTOs;

public class MarketingCampaignDto
{
    public Guid Id { get; set; }
    public string CampaignCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public MarketingChannel Channel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public Guid? PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public bool IsActive { get; set; }
    public int LeadsCount { get; set; }
    public int ConversionsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static MarketingCampaignDto FromEntity(MarketingCampaign campaign) => new()
    {
        Id = campaign.Id,
        CampaignCode = campaign.CampaignCode,
        Name = campaign.Name,
        Channel = campaign.Channel,
        StartDate = campaign.StartDate,
        EndDate = campaign.EndDate,
        Budget = campaign.Budget,
        ActualCost = campaign.ActualCost,
        PropertyId = campaign.PropertyId,
        PropertyName = campaign.Property?.PropertyName,
        AgentId = campaign.AgentId,
        AgentNameAr = campaign.Agent?.NameAr,
        IsActive = campaign.IsActive,
        LeadsCount = campaign.Leads?.Count(l => !l.IsDeleted) ?? 0,
        ConversionsCount = campaign.Leads?.Count(l => !l.IsDeleted && l.Status == LeadStatus.Converted) ?? 0,
        CreatedAt = campaign.CreatedAt
    };
}
