using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sellers.Commands.DeleteSeller;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteSellerCommand(Guid Id) : IRequest;

public class DeleteSellerCommandHandler : IRequestHandler<DeleteSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSellerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.Sellers
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Seller), request.Id);

        seller.IsDeleted = true;
        seller.DeletedAt = DateTime.UtcNow;
        seller.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
