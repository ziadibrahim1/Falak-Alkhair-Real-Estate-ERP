using FalakAlkhair.Application.Commissions.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Commissions.Queries.GetCommissionById;

public record GetCommissionByIdQuery(Guid Id) : IRequest<CommissionDto>;

public class GetCommissionByIdQueryHandler : IRequestHandler<GetCommissionByIdQuery, CommissionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCommissionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CommissionDto> Handle(GetCommissionByIdQuery request, CancellationToken cancellationToken)
    {
        var commission = await _context.Commissions
            .AsNoTracking()
            .Include(c => c.Agent)
            .Include(c => c.Lease)
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Commission), request.Id);

        return CommissionDto.FromEntity(commission);
    }
}
