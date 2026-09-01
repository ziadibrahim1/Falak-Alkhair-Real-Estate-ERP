namespace FalakAlkhair.Domain.Common;

/// <summary>
/// الأساس الموحّد لكل الكيانات: معرف + بيانات إنشاء/تعديل.
/// Base for every entity: identity + creation/modification audit fields.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
