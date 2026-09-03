using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Viewings.Commands.CreateViewing;

public record CreateViewingCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public Guid? ListingId { get; init; }
    public Guid? BuyerId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? Notes { get; init; }
}

public class CreateViewingCommandValidator : AbstractValidator<CreateViewingCommand>
{
    public CreateViewingCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.ScheduledAt).NotEmpty();
        RuleFor(x => x).Must(x => x.BuyerId.HasValue || x.TenantId.HasValue)
            .WithMessage("يجب تحديد مشترٍ أو مستأجر محتمل للمعاينة.")
            .WithName("BuyerId");
    }
}

public class CreateViewingCommandHandler : IRequestHandler<CreateViewingCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateViewingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateViewingCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var unit = await _context.Units
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        if (unit.PropertyId != request.PropertyId)
        {
            throw new Common.Exceptions.BusinessRuleException("الوحدة المحددة لا تنتمي للعقار المحدد.");
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("VIEW", companyId, cancellationToken);

        var viewing = new Viewing
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            ViewingCode = code,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            ListingId = request.ListingId,
            BuyerId = request.BuyerId,
            TenantId = request.TenantId,
            AgentId = request.AgentId,
            ScheduledAt = request.ScheduledAt,
            Status = Domain.Common.Enums.ViewingStatus.Scheduled,
            Notes = request.Notes
        };

        _context.Viewings.Add(viewing);
        await _context.SaveChangesAsync(cancellationToken);

        return viewing.Id;
    }
}
