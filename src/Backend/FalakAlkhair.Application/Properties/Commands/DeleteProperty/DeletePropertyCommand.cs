using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Properties.Commands.DeleteProperty;

public record DeletePropertyCommand(Guid Id) : IRequest;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeletePropertyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Units)
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Property), request.Id);

        if (property.Units.Any(u => !u.IsDeleted))
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف عقار يحتوي على وحدات قائمة.");
        }

        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;
        property.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
