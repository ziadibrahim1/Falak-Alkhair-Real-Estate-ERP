using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Offers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Offers.Queries.GetOfferById;

public record GetOfferByIdQuery(Guid Id) : IRequest<OfferDto>;

public class GetOfferByIdQueryHandler : IRequestHandler<GetOfferByIdQuery, OfferDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOfferByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OfferDto> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _context.Offers
            .AsNoTracking()
            .Include(o => o.Buyer)
            .Include(o => o.Property)
            .Include(o => o.Unit)
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Offer), request.Id);

        return OfferDto.FromEntity(offer);
    }
}
