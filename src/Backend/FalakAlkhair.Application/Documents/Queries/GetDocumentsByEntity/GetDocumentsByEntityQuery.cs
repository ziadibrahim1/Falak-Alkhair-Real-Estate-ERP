using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Documents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Documents.Queries.GetDocumentsByEntity;

/// <summary>مستندات كيان محدَّد (مثال: كل مستندات عقار معيَّن)، الأحدث أولًا.</summary>
public record GetDocumentsByEntityQuery(string EntityType, Guid EntityId) : IRequest<List<DocumentDto>>;

public class GetDocumentsByEntityQueryHandler : IRequestHandler<GetDocumentsByEntityQuery, List<DocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentsByEntityQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsByEntityQuery request, CancellationToken cancellationToken)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == _currentUser.CompanyId && !d.IsDeleted
                && d.EntityType == request.EntityType && d.EntityId == request.EntityId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => DocumentDto.FromEntity(d))
            .ToListAsync(cancellationToken);
    }
}
