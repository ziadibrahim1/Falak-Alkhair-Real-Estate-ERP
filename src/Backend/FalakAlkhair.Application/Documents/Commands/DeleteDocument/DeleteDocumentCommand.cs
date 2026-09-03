using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Documents.Commands.DeleteDocument;

/// <summary>حذف ناعم (Soft Delete) للسجل + حذف فعلي للملف من القرص بعد نجاح الحذف الناعم.</summary>
public record DeleteDocumentCommand(Guid Id) : IRequest;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public DeleteDocumentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Where(d => d.CompanyId == _currentUser.CompanyId && !d.IsDeleted)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Document), request.Id);

        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        document.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);

        // حذف الملف الفعلي من القرص بعد نجاح الحذف الناعم للسجل — لا عكس، حتى لا يُترَك
        // سجل يشير لملف محذوف فعليًا إن فشل حذف القرص بعده.
        await _fileStorage.DeleteAsync(document.FilePath, cancellationToken);
    }
}
