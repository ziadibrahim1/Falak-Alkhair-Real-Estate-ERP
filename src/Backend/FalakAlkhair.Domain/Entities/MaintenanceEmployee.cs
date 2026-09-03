using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>فني/موظف صيانة داخلي يمكن إسناد طلبات الصيانة إليه.</summary>
public class MaintenanceEmployee : BaseAuditableEntity
{
    public string EmployeeCode { get; set; } = default!; // EMP-000001

    public string NameAr { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string? Email { get; set; }
    public string? Department { get; set; }
    public string? Skills { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<MaintenanceRequest> AssignedRequests { get; set; } = new List<MaintenanceRequest>();
}
