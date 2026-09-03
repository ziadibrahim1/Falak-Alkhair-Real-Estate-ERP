namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// تخزين الملفات الفعلي للمستندات المرفوعة. التنفيذ الحالي (Infrastructure)
/// يحفظ على القرص المحلي خارج wwwroot عمدًا — لا تُقدَّم المستندات كملفات
/// ثابتة عامة، بل حصرًا عبر نقطة تحميل محمية بصلاحيات ونطاق شركة
/// (DownloadDocumentQuery)، لتفادي كشف مستندات حسّاسة (صكوك، هويات) لأي زائر.
/// </summary>
public interface IFileStorageService
{
    /// <summary>يحفظ الملف تحت مسار فرعي منطقي (مثال: "{companyId}/Property/{propertyId}") ويعيد المسار النسبي المخزَّن + حجم الملف.</summary>
    Task<(string RelativePath, long FileSize)> SaveAsync(Stream content, string fileName, string subPath, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}
