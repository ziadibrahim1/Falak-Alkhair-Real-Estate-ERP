using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leads.Commands.UpdateLead;

public record UpdateLeadCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public LeadSource Source { get; init; }
    public LeadType LeadType { get; init; }
    public Guid? InterestedPropertyId { get; init; }
    public LeadStatus Status { get; init; }
    public LeadPriority Priority { get; init; }
    public string? Notes { get; init; }
}

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateLeadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lead), request.Id);

        lead.NameAr = request.NameAr;
        lead.Mobile = request.Mobile;
        lead.Email = request.Email;
        lead.Source = request.Source;
        lead.LeadType = request.LeadType;
        lead.InterestedPropertyId = request.InterestedPropertyId;
        lead.Status = request.Status;
        lead.Priority = request.Priority;
        lead.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
