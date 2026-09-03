using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Documents.Commands.UploadDocument;

/// <summary>
/// رفع مستند فعلي وربطه بأي كيان عبر EntityType/EntityId (Polymorphic).
/// المحتوى (Stream) يصل من الـ Controller (ASP.NET Core IFormFile.OpenReadStream)
/// دون أن تعتمد هذه الطبقة على أي نوع خاص بـ ASP.NET Core.
/// </summary>
public record UploadDocumentCommand : IRequest<Guid>
{
    public Stream FileContent { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public string MimeType { get; init; } = default!;
    public long FileSize { get; init; }
    public string DocumentType { get; init; } = default!;
    public string EntityType { get; init; } = default!;
    public Guid EntityId { get; init; }
    public string? Notes { get; init; }
    public DateTime? ExpiryDate { get; init; }
}

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    // القائمة البيضاء لامتدادات الملفات المسموحة ومصفوفة أنواع MIME المقابلة —
    // تحديدًا لمنع رفع ملفات تنفيذية (OWASP Unrestricted File Upload).
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx"
    };

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 ميغابايت — يطابق FileStorageSettings الافتراضي في Infrastructure.

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(300)
            .Must(fn => AllowedExtensions.Contains(Path.GetExtension(fn)))
            .WithMessage($"امتداد الملف غير مسموح به. الامتدادات المسموحة: {string.Join(", ", AllowedExtensions)}");
        RuleFor(x => x.FileSize).GreaterThan(0).LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("حجم الملف يجب ألا يتجاوز 20 ميغابايت.");
        RuleFor(x => x.MimeType).NotEmpty();
    }
}

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadDocumentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var subPath = $"{companyId:N}/{request.EntityType}/{request.EntityId:N}";

        var (relativePath, fileSize) = await _fileStorage.SaveAsync(request.FileContent, request.FileName, subPath, cancellationToken);

        var document = new Document
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            DocumentType = request.DocumentType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            FileName = request.FileName,
            FilePath = relativePath,
            FileSize = fileSize,
            MimeType = request.MimeType,
            Notes = request.Notes,
            ExpiryDate = request.ExpiryDate
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}
