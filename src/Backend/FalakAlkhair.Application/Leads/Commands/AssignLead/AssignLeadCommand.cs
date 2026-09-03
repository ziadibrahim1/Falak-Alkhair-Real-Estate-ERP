using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leads.Commands.AssignLead;

/// <summary>إسناد عميل محتمل لمسوّق عقاري (يتحقق أن المسوّق فعّال قبل الإسناد).</summary>
public record AssignLeadCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
}

public class AssignLeadCommandValidator : AbstractValidator<AssignLeadCommand>
{
    public AssignLeadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AgentId).NotEmpty();
    }
}

public class AssignLeadCommandHandler : IRequestHandler<AssignLeadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AssignLeadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lead), request.Id);

        var agentExists = await _context.Agents
            .AnyAsync(a => a.Id == request.AgentId && a.CompanyId == _currentUser.CompanyId && !a.IsDeleted && a.IsActive, cancellationToken);
        if (!agentExists)
        {
            throw new NotFoundException(nameof(Domain.Entities.Agent), request.AgentId);
        }

        lead.AssignedAgentId = request.AgentId;
        if (lead.Status == Domain.Common.Enums.LeadStatus.New)
        {
            lead.Status = Domain.Common.Enums.LeadStatus.Contacted;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
