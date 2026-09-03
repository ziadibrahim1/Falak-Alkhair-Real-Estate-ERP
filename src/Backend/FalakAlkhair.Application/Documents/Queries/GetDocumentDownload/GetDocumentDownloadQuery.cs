using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Documents.Queries.GetDocumentDownload;

public record DocumentDownloadResult(Stream Content, string FileName, string MimeType);

/// <summary>
/// يعيد دفق قراءة الملف الفعلي بعد التحقق من ملكية السجل لنفس شركة المستخدم
/// الحالي — هذه هي النقطة الوحيدة التي يُقدَّم منها محتوى المستند فعليًا،
/// عمدًا وليس عبر ملفات ثابتة عامة (راجع IFileStorageService).
/// </summary>
public record GetDocumentDownloadQuery(Guid Id) : IRequest<DocumentDownloadResult>;

public class GetDocumentDownloadQueryHandler : IRequestHandler<GetDocumentDownloadQuery, DocumentDownloadResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public GetDocumentDownloadQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<DocumentDownloadResult> Handle(GetDocumentDownloadQuery request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == _currentUser.CompanyId && !d.IsDeleted)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Document), request.Id);

        var stream = await _fileStorage.OpenReadAsync(document.FilePath, cancellationToken);
        return new DocumentDownloadResult(stream, document.FileName, document.MimeType);
    }
}
