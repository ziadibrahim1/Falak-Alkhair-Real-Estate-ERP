namespace FalakAlkhair.Domain.Common;

/// <summary>
/// أساس الكيانات متعددة الشركات/الفروع مع دعم الحذف الناعم (Soft Delete).
/// Base for multi-company/multi-branch, soft-deletable business entities.
/// كل سجل عمل مهم في النظام يرث من هذا الكيان ليكون قابلًا للربط بالشركة والفرع
/// وقابلًا للتدقيق (Audit) والحذف الناعم بدل الحذف الفعلي.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }
}
