namespace FalakAlkhair.Infrastructure.Files;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// مسار جذر تخزين المستندات. نسبي إلى ContentRootPath إن لم يكن مطلقًا.
    /// خارج wwwroot عمدًا — راجع IFileStorageService لسبب ذلك.
    /// </summary>
    public string RootPath { get; set; } = "App_Data/documents";

    /// <summary>الحجم الأقصى للملف الواحد بالبايت (افتراضيًا 20 ميغابايت).</summary>
    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
}
