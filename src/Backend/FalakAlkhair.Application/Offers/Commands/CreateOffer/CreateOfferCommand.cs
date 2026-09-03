using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Offers.Commands.CreateOffer;

public record CreateOfferCommand : IRequest<Guid>
{
    public Guid BuyerId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public decimal Amount { get; init; }
    public DateTime OfferDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Conditions { get; init; }
    public string? Notes { get; init; }
}

public class CreateOfferCommandValidator : AbstractValidator<CreateOfferCommand>
{
    public CreateOfferCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("قيمة العرض يجب أن تكون أكبر من صفر.");
        RuleFor(x => x.ExpirationDate).GreaterThan(x => x.OfferDate).When(x => x.ExpirationDate.HasValue)
            .WithMessage("تاريخ انتهاء العرض يجب أن يكون بعد تاريخ تقديمه.");
    }
}

public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateOfferCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
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

        var code = await _numberGenerator.GenerateNextNumberAsync("OFFER", companyId, cancellationToken);

        var offer = new Offer
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            OfferNumber = code,
            BuyerId = request.BuyerId,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            Amount = request.Amount,
            OfferDate = request.OfferDate,
            ExpirationDate = request.ExpirationDate,
            Conditions = request.Conditions,
            Status = OfferStatus.Pending,
            Notes = request.Notes
        };

        _context.Offers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }
}
