using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.MaintenanceEmployees.DTOs;

public class MaintenanceEmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public string? Department { get; set; }
    public string? Skills { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int AssignedRequestsCount { get; set; }

    public static MaintenanceEmployeeDto FromEntity(MaintenanceEmployee employee) => new()
    {
        Id = employee.Id,
        EmployeeCode = employee.EmployeeCode,
        NameAr = employee.NameAr,
        Mobile = employee.Mobile,
        Email = employee.Email,
        Department = employee.Department,
        Skills = employee.Skills,
        IsAvailable = employee.IsAvailable,
        IsActive = employee.IsActive,
        AssignedRequestsCount = employee.AssignedRequests?.Count(r => !r.IsDeleted) ?? 0
    };
}
