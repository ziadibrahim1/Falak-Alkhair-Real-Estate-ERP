using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Listings.Commands.DeleteListing;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteListingCommand(Guid Id) : IRequest;

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteListingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Listing), request.Id);

        listing.IsDeleted = true;
        listing.DeletedAt = DateTime.UtcNow;
        listing.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
