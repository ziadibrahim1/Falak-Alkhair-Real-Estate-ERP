namespace FalakAlkhair.Domain.Entities;

/// <summary>
/// عدّاد مركزي لتوليد الأرقام المرجعية (PROP-000001، LEASE-000001 ...) بأمان
/// تحت التزامن العالي. يُقرأ ويُحدَّث ضمن معاملة قاعدة بيانات مع قفل صفّي
/// (Row Lock / UPDLOCK) في NumberGeneratorService بطبقة Infrastructure.
/// </summary>
public class NumberSequence
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>مفتاح النوع، مثال: "PROPERTY"، "UNIT"، "LEASE".</summary>
    public string EntityKey { get; set; } = default!;

    public string Prefix { get; set; } = default!;

    public long CurrentNumber { get; set; }

    public int PaddingLength { get; set; } = 6;

    public Guid CompanyId { get; set; }

    /// <summary>مستخدم داخليًا لضمان تفرّد الصف عبر EF (Concurrency Token).</summary>
    public byte[]? RowVersion { get; set; }
}
