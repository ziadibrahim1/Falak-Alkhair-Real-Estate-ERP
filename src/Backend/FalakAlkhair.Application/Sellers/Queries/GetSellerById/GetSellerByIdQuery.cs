using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Sellers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sellers.Queries.GetSellerById;

public record GetSellerByIdQuery(Guid Id) : IRequest<SellerDto>;

public class GetSellerByIdQueryHandler : IRequestHandler<GetSellerByIdQuery, SellerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSellerByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SellerDto> Handle(GetSellerByIdQuery request, CancellationToken cancellationToken)
    {
        var seller = await _context.Sellers
            .AsNoTracking()
            .Include(s => s.Owner)
            .Include(s => s.Property)
            .Include(s => s.AssignedAgent)
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Seller), request.Id);

        return SellerDto.FromEntity(seller);
    }
}
