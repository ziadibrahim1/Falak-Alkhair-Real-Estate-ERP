using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Listings.Commands.CreateListing;

public record CreateListingCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public ListingType ListingType { get; init; }
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public string? Features { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime? ListingStartDate { get; init; }
    public DateTime? ListingEndDate { get; init; }
}

public class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    public CreateListingCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("سعر الإعلان يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.ListingEndDate).GreaterThan(x => x.ListingStartDate)
            .When(x => x.ListingStartDate.HasValue && x.ListingEndDate.HasValue)
            .WithMessage("تاريخ انتهاء الإعلان يجب أن يكون بعد تاريخ البداية.");
    }
}

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateListingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateListingCommand request, CancellationToken cancellationToken)
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

        var code = await _numberGenerator.GenerateNextNumberAsync("LIST", companyId, cancellationToken);

        var listing = new Listing
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            ListingCode = code,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            ListingType = request.ListingType,
            Price = request.Price,
            Description = request.Description,
            Features = request.Features,
            AgentId = request.AgentId,
            ListingStartDate = request.ListingStartDate,
            ListingEndDate = request.ListingEndDate,
            Status = ListingStatus.Draft
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return listing.Id;
    }
}
