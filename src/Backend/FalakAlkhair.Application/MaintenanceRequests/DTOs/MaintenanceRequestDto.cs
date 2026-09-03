using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.MaintenanceRequests.DTOs;

public class MaintenanceRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public string? TenantNameAr { get; set; }
    public MaintenanceRequestType RequestType { get; set; }
    public MaintenancePriority Priority { get; set; }
    public string Description { get; set; } = default!;
    public Guid? AssignedEmployeeId { get; set; }
    public string? AssignedEmployeeNameAr { get; set; }
    public Guid? AssignedVendorId { get; set; }
    public string? AssignedVendorNameAr { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public MaintenanceStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public static MaintenanceRequestDto FromEntity(MaintenanceRequest request) => new()
    {
        Id = request.Id,
        RequestNumber = request.RequestNumber,
        PropertyId = request.PropertyId,
        PropertyName = request.Property?.PropertyName ?? string.Empty,
        UnitId = request.UnitId,
        UnitNumber = request.Unit?.UnitNumber ?? string.Empty,
        TenantId = request.TenantId,
        TenantNameAr = request.Tenant?.NameAr,
        RequestType = request.RequestType,
        Priority = request.Priority,
        Description = request.Description,
        AssignedEmployeeId = request.AssignedEmployeeId,
        AssignedEmployeeNameAr = request.AssignedEmployee?.NameAr,
        AssignedVendorId = request.AssignedVendorId,
        AssignedVendorNameAr = request.AssignedVendor?.NameAr,
        EstimatedCost = request.EstimatedCost,
        ActualCost = request.ActualCost,
        Status = request.Status,
        StartDate = request.StartDate,
        CompletionDate = request.CompletionDate,
        CreatedAt = request.CreatedAt
    };
}
