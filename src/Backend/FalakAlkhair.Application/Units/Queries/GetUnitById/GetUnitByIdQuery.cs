using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Units.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Units.Queries.GetUnitById;

public record GetUnitByIdQuery(Guid Id) : IRequest<UnitDto>;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnitByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UnitDto> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units
            .AsNoTracking()
            .Include(u => u.Property)
            .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        return UnitDto.FromEntity(unit);
    }
}
