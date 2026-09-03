using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Documents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Documents.Queries.GetDocumentsList;

/// <summary>قائمة عامة بكل مستندات الشركة (بحث/تصفّح)، تُستخدم في شاشة "المستندات" المركزية.</summary>
public class GetDocumentsListQuery : ListQueryParams, IRequest<PaginatedList<DocumentDto>>
{
    public string? EntityType { get; init; }
}

public class GetDocumentsListQueryHandler : IRequestHandler<GetDocumentsListQuery, PaginatedList<DocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<DocumentDto>> Handle(GetDocumentsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == _currentUser.CompanyId && !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(d => d.EntityType == request.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(d => d.FileName.Contains(term) || d.DocumentType.Contains(term));
        }

        query = request.SortDescending
            ? query.OrderByDescending(d => d.CreatedAt)
            : query.OrderBy(d => d.CreatedAt);

        var projected = query.Select(d => DocumentDto.FromEntity(d));

        return await PaginatedList<DocumentDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
