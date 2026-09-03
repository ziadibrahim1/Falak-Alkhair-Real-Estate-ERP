using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Listings.Commands.UpdateListing;

public record UpdateListingCommand : IRequest
{
    public Guid Id { get; init; }
    public ListingType ListingType { get; init; }
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public string? Features { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime? ListingStartDate { get; init; }
    public DateTime? ListingEndDate { get; init; }
}

public class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    public UpdateListingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.ListingEndDate).GreaterThan(x => x.ListingStartDate)
            .When(x => x.ListingStartDate.HasValue && x.ListingEndDate.HasValue);
    }
}

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateListingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Listing), request.Id);

        listing.ListingType = request.ListingType;
        listing.Price = request.Price;
        listing.Description = request.Description;
        listing.Features = request.Features;
        listing.AgentId = request.AgentId;
        listing.ListingStartDate = request.ListingStartDate;
        listing.ListingEndDate = request.ListingEndDate;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
