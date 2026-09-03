using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sellers.Commands.CreateSeller;

public record CreateSellerCommand : IRequest<Guid>
{
    public Guid OwnerId { get; init; }
    public Guid? PropertyId { get; init; }
    public decimal AskingPrice { get; init; }
    public decimal? MinimumPrice { get; init; }
    public decimal CommissionPercentage { get; init; }
    public DateTime MandateStartDate { get; init; }
    public DateTime? MandateEndDate { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public string? Notes { get; init; }
}

public class CreateSellerCommandValidator : AbstractValidator<CreateSellerCommand>
{
    public CreateSellerCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("المالك مطلوب.");
        RuleFor(x => x.AskingPrice).GreaterThan(0).WithMessage("سعر الطلب يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.MinimumPrice).LessThanOrEqualTo(x => x.AskingPrice)
            .When(x => x.MinimumPrice.HasValue)
            .WithMessage("الحد الأدنى للسعر يجب ألا يتجاوز سعر الطلب.");
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MandateEndDate).GreaterThan(x => x.MandateStartDate)
            .When(x => x.MandateEndDate.HasValue)
            .WithMessage("تاريخ انتهاء التفويض يجب أن يكون بعد تاريخ البداية.");
    }
}

public class CreateSellerCommandHandler : IRequestHandler<CreateSellerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateSellerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateSellerCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var ownerExists = await _context.Owners
            .AnyAsync(o => o.Id == request.OwnerId && o.CompanyId == companyId && !o.IsDeleted, cancellationToken);
        if (!ownerExists)
        {
            throw new NotFoundException(nameof(Owner), request.OwnerId);
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("SELLER", companyId, cancellationToken);

        var seller = new Seller
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            SellerCode = code,
            OwnerId = request.OwnerId,
            PropertyId = request.PropertyId,
            AskingPrice = request.AskingPrice,
            MinimumPrice = request.MinimumPrice,
            CommissionPercentage = request.CommissionPercentage,
            MandateStatus = ListingMandateStatus.Draft,
            MandateStartDate = request.MandateStartDate,
            MandateEndDate = request.MandateEndDate,
            AssignedAgentId = request.AssignedAgentId,
            Notes = request.Notes
        };

        _context.Sellers.Add(seller);
        await _context.SaveChangesAsync(cancellationToken);

        return seller.Id;
    }
}
