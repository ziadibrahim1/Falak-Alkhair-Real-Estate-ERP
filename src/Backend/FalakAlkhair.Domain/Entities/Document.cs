using FalakAlkhair.Domain.Common;

namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// مستند مرتبط بأي كيان في النظام (صك، عقد، هوية، رخصة ...) عبر
/// EntityType/EntityId بنمط Polymorphic Association بسيط وواضح بدل جداول
/// مستقلة لكل نوع مستند.
/// </summary>
public class Document : BaseAuditableEntity
{
    /// <summary>نوع المستند: صك ملكية، عقد، هوية، رخصة فال، اتفاقية، فاتورة ...</summary>
    public string DocumentType { get; set; } = default!;

    /// <summary>اسم الكيان المرتبط، مثال: "Property"، "Owner"، "Lease".</summary>
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }

    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = default!;

    public string? Notes { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
