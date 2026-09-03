using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agents.Commands.UpdateAgent;

public record UpdateAgentCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? FalLicenseNumber { get; init; }
    public DateTime? FalLicenseExpiryDate { get; init; }
    public string? Specialization { get; init; }
    public Guid? ManagerUserId { get; init; }
    public AgentStatus Status { get; init; }
    public CommissionType CommissionSchemeType { get; init; }
    public decimal DefaultCommissionPercentage { get; init; }
    public decimal? DefaultCommissionFixedAmount { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateAgentCommandValidator : AbstractValidator<UpdateAgentCommand>
{
    public UpdateAgentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.DefaultCommissionPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAgentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _context.Agents
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Agent), request.Id);

        agent.NameAr = request.NameAr;
        agent.NameEn = request.NameEn;
        agent.Mobile = request.Mobile;
        agent.Email = request.Email;
        agent.FalLicenseNumber = request.FalLicenseNumber;
        agent.FalLicenseExpiryDate = request.FalLicenseExpiryDate;
        agent.Specialization = request.Specialization;
        agent.ManagerUserId = request.ManagerUserId;
        agent.Status = request.Status;
        agent.CommissionSchemeType = request.CommissionSchemeType;
        agent.DefaultCommissionPercentage = request.DefaultCommissionPercentage;
        agent.DefaultCommissionFixedAmount = request.DefaultCommissionFixedAmount;
        agent.Notes = request.Notes;
        agent.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
