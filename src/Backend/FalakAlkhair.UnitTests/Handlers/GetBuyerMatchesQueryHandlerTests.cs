using FalakAlkhair.Application.Buyers.Queries.GetBuyerMatches;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class GetBuyerMatchesQueryHandlerTests
{
    [Fact]
    public async Task Should_Return_Only_Units_ListedForSale_Within_Budget_And_City()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;

        var owner = new Owner { CompanyId = companyId, OwnerCode = "OWNER-000001", NameAr = "مالك", Mobile = "0500000000" };
        context.Owners.Add(owner);

        var matchingProperty = new Property
        {
            CompanyId = companyId, PropertyCode = "PROP-000001", PropertyName = "عمارة الرياض",
            PropertyType = PropertyType.Apartment, PropertyCategory = PropertyCategory.Residential,
            OwnerId = owner.Id, City = "الرياض"
        };
        var otherCityProperty = new Property
        {
            CompanyId = companyId, PropertyCode = "PROP-000002", PropertyName = "عمارة جدة",
            PropertyType = PropertyType.Apartment, PropertyCategory = PropertyCategory.Residential,
            OwnerId = owner.Id, City = "جدة"
        };
        context.Properties.AddRange(matchingProperty, otherCityProperty);

        var matchingUnit = new Unit
        {
            CompanyId = companyId, PropertyId = matchingProperty.Id, UnitCode = "UNIT-000001", UnitNumber = "1",
            UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.ListedForSale, SalePrice = 800_000, Area = 150
        };
        var tooExpensiveUnit = new Unit
        {
            CompanyId = companyId, PropertyId = matchingProperty.Id, UnitCode = "UNIT-000002", UnitNumber = "2",
            UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.ListedForSale, SalePrice = 5_000_000, Area = 150
        };
        var wrongCityUnit = new Unit
        {
            CompanyId = companyId, PropertyId = otherCityProperty.Id, UnitCode = "UNIT-000003", UnitNumber = "3",
            UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.ListedForSale, SalePrice = 800_000, Area = 150
        };
        var notListedUnit = new Unit
        {
            CompanyId = companyId, PropertyId = matchingProperty.Id, UnitCode = "UNIT-000004", UnitNumber = "4",
            UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.Rented, SalePrice = 800_000, Area = 150
        };
        context.Units.AddRange(matchingUnit, tooExpensiveUnit, wrongCityUnit, notListedUnit);

        var buyer = new Buyer
        {
            CompanyId = companyId, BuyerCode = "BUYER-000001", NameAr = "مشترٍ", Mobile = "0511111111",
            Budget = 1_000_000, PreferredCity = "الرياض"
        };
        context.Buyers.Add(buyer);

        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetBuyerMatchesQueryHandler(context, currentUser);
        var matches = await handler.Handle(new GetBuyerMatchesQuery(buyer.Id), CancellationToken.None);

        matches.Should().ContainSingle();
        matches[0].UnitId.Should().Be(matchingUnit.Id);
    }
}
