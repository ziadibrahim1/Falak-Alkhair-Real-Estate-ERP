using FalakAlkhair.Application.Auctions.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IRequest<AuctionDto>;

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuctionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AuctionDto> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .AsNoTracking()
            .Include(a => a.Property)
            .Include(a => a.Unit)
            .Include(a => a.Owner)
            .Include(a => a.Agent)
            .Include(a => a.WinnerBuyer)
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Auction), request.Id);

        return AuctionDto.FromEntity(auction);
    }
}
