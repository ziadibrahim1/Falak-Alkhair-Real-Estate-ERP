using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Listings.Commands.PublishListing;

/// <summary>
/// نشر إعلان عقاري: Draft/PendingReview/Paused → Published. يمنع النشر بدون
/// البيانات المطلوبة (السعر، الوصف)، ويحدّث حالة الوحدة تلقائيًا إلى
/// ListedForSale/ListedForRent حسب نوع الإعلان — أثر جانبي حقيقي على المخزون.
/// </summary>
public record PublishListingCommand(Guid Id) : IRequest;

public class PublishListingCommandHandler : IRequestHandler<PublishListingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublishListingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Listing), request.Id);

        if (listing.Status is not (ListingStatus.Draft or ListingStatus.PendingReview or ListingStatus.Paused))
        {
            throw new Common.Exceptions.BusinessRuleException(
                $"لا يمكن نشر إعلان بحالته الحالية ({listing.Status}).");
        }

        if (listing.Price <= 0 || string.IsNullOrWhiteSpace(listing.Description))
        {
            throw new Common.Exceptions.BusinessRuleException(
                "لا يمكن نشر الإعلان دون استيفاء البيانات المطلوبة (السعر والوصف).");
        }

        listing.Status = ListingStatus.Published;
        listing.ListingStartDate ??= DateTime.UtcNow;

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == listing.UnitId, cancellationToken);
        if (unit is not null)
        {
            unit.CurrentStatus = listing.ListingType == ListingType.ForSale
                ? UnitStatus.ListedForSale
                : UnitStatus.ListedForRent;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
