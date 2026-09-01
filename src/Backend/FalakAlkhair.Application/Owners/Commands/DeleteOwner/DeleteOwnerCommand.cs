using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Owners.Commands.DeleteOwner;

/// <summary>حذف ناعم (Soft Delete) فقط — لا يُحذف أي سجل مالي/عقاري فعليًا من قاعدة البيانات.</summary>
public record DeleteOwnerCommand(Guid Id) : IRequest;

public class DeleteOwnerCommandHandler : IRequestHandler<DeleteOwnerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteOwnerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _context.Owners
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted)
            .Include(o => o.Properties)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Owner), request.Id);

        if (owner.Properties.Any())
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف مالك مرتبط بعقارات قائمة. أوقف حالته بدلًا من ذلك.");
        }

        owner.IsDeleted = true;
        owner.DeletedAt = DateTime.UtcNow;
        owner.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
