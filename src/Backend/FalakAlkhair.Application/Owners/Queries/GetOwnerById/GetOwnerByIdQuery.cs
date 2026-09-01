using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Owners.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery(Guid Id) : IRequest<OwnerDto>;

public class GetOwnerByIdQueryHandler : IRequestHandler<GetOwnerByIdQuery, OwnerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOwnerByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OwnerDto> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await _context.Owners
            .AsNoTracking()
            .Include(o => o.Properties)
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Owner), request.Id);

        return OwnerDto.FromEntity(owner);
    }
}
