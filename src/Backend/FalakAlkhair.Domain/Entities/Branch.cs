using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>فرع تابع لشركة (Multi-Branch Ready).</summary>
public class Branch : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;

    public string Code { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMainBranch { get; set; }
}
