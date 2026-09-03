using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Vendors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Vendors.Queries.GetVendorById;

public record GetVendorByIdQuery(Guid Id) : IRequest<VendorDto>;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetVendorByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VendorDto> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .AsNoTracking()
            .Include(v => v.AssignedRequests)
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vendor), request.Id);

        return VendorDto.FromEntity(vendor);
    }
}
