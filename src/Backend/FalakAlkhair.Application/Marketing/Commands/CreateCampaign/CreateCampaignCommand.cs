using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Marketing.Commands.CreateCampaign;

public record CreateCampaignCommand : IRequest<Guid>
{
    public string Name { get; init; } = default!;
    public MarketingChannel Channel { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal Budget { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? AgentId { get; init; }
    public string? Notes { get; init; }
}

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم الحملة مطلوب.").MaximumLength(200);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("تاريخ نهاية الحملة يجب أن يكون بعد تاريخ البداية.");
    }
}

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateCampaignCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("CAMP", companyId, cancellationToken);

        var campaign = new MarketingCampaign
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            CampaignCode = code,
            Name = request.Name,
            Channel = request.Channel,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            PropertyId = request.PropertyId,
            AgentId = request.AgentId,
            Notes = request.Notes,
            IsActive = true
        };

        _context.MarketingCampaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return campaign.Id;
    }
}
