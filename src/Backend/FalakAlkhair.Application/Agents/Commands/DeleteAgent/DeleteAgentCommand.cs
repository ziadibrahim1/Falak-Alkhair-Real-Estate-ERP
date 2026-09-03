using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agents.Commands.DeleteAgent;

/// <summary>حذف ناعم (Soft Delete) فقط — لا يُحذف أي سجل عمولات مرتبط فعليًا.</summary>
public record DeleteAgentCommand(Guid Id) : IRequest;

public class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteAgentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _context.Agents
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .Include(a => a.Commissions)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Agent), request.Id);

        if (agent.Commissions.Any())
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف مسوّق مرتبط بعمولات مسجّلة. أوقف حالته بدلًا من ذلك.");
        }

        agent.IsDeleted = true;
        agent.DeletedAt = DateTime.UtcNow;
        agent.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
