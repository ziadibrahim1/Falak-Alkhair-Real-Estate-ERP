using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Buyers.Commands.DeleteBuyer;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteBuyerCommand(Guid Id) : IRequest;

public class DeleteBuyerCommandHandler : IRequestHandler<DeleteBuyerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteBuyerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await _context.Buyers
            .Where(b => b.CompanyId == _currentUser.CompanyId && !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Buyer), request.Id);

        buyer.IsDeleted = true;
        buyer.DeletedAt = DateTime.UtcNow;
        buyer.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
