using FalakAlkhair.Application.Buyers.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Buyers.Queries.GetBuyerById;

public record GetBuyerByIdQuery(Guid Id) : IRequest<BuyerDto>;

public class GetBuyerByIdQueryHandler : IRequestHandler<GetBuyerByIdQuery, BuyerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBuyerByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BuyerDto> Handle(GetBuyerByIdQuery request, CancellationToken cancellationToken)
    {
        var buyer = await _context.Buyers
            .AsNoTracking()
            .Include(b => b.AssignedAgent)
            .Where(b => b.CompanyId == _currentUser.CompanyId && !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Buyer), request.Id);

        return BuyerDto.FromEntity(buyer);
    }
}
