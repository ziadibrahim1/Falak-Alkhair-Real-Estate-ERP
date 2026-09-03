using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FalakAlkhair.Infrastructure.Files;

/// <summary>
/// تخزين محلي حقيقي على القرص (وليس Mock) خارج wwwroot، خلف IFileStorageService
/// القابل للاستبدال لاحقًا بمزوّد سحابي (S3 / Azure Blob) دون تغيير طبقة
/// Application. يمنع صراحة الخروج عن المجلد الجذر (Path Traversal) عبر التحقق
/// من المسار المطلق النهائي بعد الدمج، ويولّد اسم ملف فريد (GUID) على القرص
/// بدل استخدام اسم الملف الأصلي مباشرة لتفادي أي حرف خطير فيه.
/// </summary>
public class LocalDiskFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalDiskFileStorageService(IHostEnvironment environment, IOptions<FileStorageSettings> settings)
    {
        var configuredRoot = settings.Value.RootPath;
        _rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string RelativePath, long FileSize)> SaveAsync(Stream content, string fileName, string subPath, CancellationToken cancellationToken)
    {
        var safeSubPath = SanitizeSubPath(subPath);
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(safeSubPath, storedFileName).Replace('\\', '/');

        var fullPath = ResolveFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var fileSize = new FileInfo(fullPath).Length;
        return (relativePath, fileSize);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("File", relativePath);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeSubPath(string subPath)
    {
        var segments = subPath
            .Split('/', '\\', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => string.Concat(s.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_')));

        return Path.Combine(segments.ToArray());
    }

    private string ResolveFullPath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        var normalizedRoot = Path.GetFullPath(_rootPath) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
        {
            throw new BusinessRuleException("مسار ملف غير صالح.");
        }

        return fullPath;
    }
}
