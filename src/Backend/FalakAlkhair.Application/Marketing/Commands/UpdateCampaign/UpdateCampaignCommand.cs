using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Marketing.Commands.UpdateCampaign;

public record UpdateCampaignCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public MarketingChannel Channel { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal Budget { get; init; }
    public decimal ActualCost { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? AgentId { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActualCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}

public class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCampaignCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.MarketingCampaigns
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MarketingCampaign), request.Id);

        campaign.Name = request.Name;
        campaign.Channel = request.Channel;
        campaign.StartDate = request.StartDate;
        campaign.EndDate = request.EndDate;
        campaign.Budget = request.Budget;
        campaign.ActualCost = request.ActualCost;
        campaign.PropertyId = request.PropertyId;
        campaign.AgentId = request.AgentId;
        campaign.Notes = request.Notes;
        campaign.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
