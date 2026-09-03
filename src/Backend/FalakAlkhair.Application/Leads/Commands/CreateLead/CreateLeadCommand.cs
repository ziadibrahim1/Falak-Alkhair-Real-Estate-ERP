using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Leads.Commands.CreateLead;

public record CreateLeadCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = default!;
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public LeadSource Source { get; init; } = LeadSource.Other;
    public LeadType LeadType { get; init; }
    public Guid? InterestedPropertyId { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public Guid? CampaignId { get; init; }
    public LeadPriority Priority { get; init; } = LeadPriority.Medium;
    public string? Notes { get; init; }
}

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم العميل المحتمل بالعربية مطلوب.").MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب.")
            .Matches(@"^(009665|9665|\+9665|05|5)([0-9]{8})$").WithMessage("رقم جوال سعودي غير صحيح.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
    }
}

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateLeadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;
        var code = await _numberGenerator.GenerateNextNumberAsync("LEAD", companyId, cancellationToken);

        var lead = new Lead
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            LeadCode = code,
            NameAr = request.NameAr,
            Mobile = request.Mobile,
            Email = request.Email,
            Source = request.Source,
            LeadType = request.LeadType,
            InterestedPropertyId = request.InterestedPropertyId,
            AssignedAgentId = request.AssignedAgentId,
            CampaignId = request.CampaignId,
            Status = LeadStatus.New,
            Priority = request.Priority,
            Notes = request.Notes
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}
