using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Properties.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto>;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPropertyByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PropertyDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Units)
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Property), request.Id);

        return PropertyDto.FromEntity(property);
    }
}
