using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Viewings.DTOs;

public class ViewingDto
{
    public Guid Id { get; set; }
    public string ViewingCode { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public Guid? BuyerId { get; set; }
    public string? BuyerNameAr { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantNameAr { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public DateTime ScheduledAt { get; set; }
    public ViewingStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Feedback { get; set; }

    public static ViewingDto FromEntity(Viewing viewing) => new()
    {
        Id = viewing.Id,
        ViewingCode = viewing.ViewingCode,
        PropertyId = viewing.PropertyId,
        PropertyName = viewing.Property?.PropertyName ?? string.Empty,
        UnitId = viewing.UnitId,
        UnitNumber = viewing.Unit?.UnitNumber ?? string.Empty,
        BuyerId = viewing.BuyerId,
        BuyerNameAr = viewing.Buyer?.NameAr,
        TenantId = viewing.TenantId,
        TenantNameAr = viewing.Tenant?.NameAr,
        AgentId = viewing.AgentId,
        AgentNameAr = viewing.Agent?.NameAr,
        ScheduledAt = viewing.ScheduledAt,
        Status = viewing.Status,
        Notes = viewing.Notes,
        Feedback = viewing.Feedback
    };
}
