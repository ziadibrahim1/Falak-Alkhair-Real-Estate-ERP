using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Listings.Commands.PublishListing;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class PublishListingCommandHandlerTests
{
    [Fact]
    public async Task Should_Reject_Publishing_Listing_Without_Description()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var listing = new Listing
        {
            CompanyId = currentUser.CompanyId!.Value,
            ListingCode = "LIST-000001",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            ListingType = ListingType.ForSale,
            Price = 800_000,
            Description = null,
            Status = ListingStatus.Draft
        };
        context.Listings.Add(listing);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new PublishListingCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(new PublishListingCommand(listing.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Publish_And_Update_Unit_Status_When_Data_Complete()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var unitId = Guid.NewGuid();

        context.Units.Add(new Unit
        {
            Id = unitId, CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "UNIT-000001",
            UnitNumber = "1", UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.Available
        });

        var listing = new Listing
        {
            CompanyId = companyId,
            ListingCode = "LIST-000002",
            PropertyId = Guid.NewGuid(),
            UnitId = unitId,
            ListingType = ListingType.ForRent,
            Price = 45_000,
            Description = "شقة مفروشة بالكامل في حي الملقا",
            Status = ListingStatus.Draft
        };
        context.Listings.Add(listing);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new PublishListingCommandHandler(context, currentUser);
        await handler.Handle(new PublishListingCommand(listing.Id), CancellationToken.None);

        var updatedListing = await context.Listings.FindAsync(listing.Id);
        updatedListing!.Status.Should().Be(ListingStatus.Published);

        var updatedUnit = await context.Units.FindAsync(unitId);
        updatedUnit!.CurrentStatus.Should().Be(UnitStatus.ListedForRent);
    }
}
