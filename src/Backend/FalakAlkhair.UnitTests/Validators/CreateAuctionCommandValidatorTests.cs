using FalakAlkhair.Application.Auctions.Commands.CreateAuction;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class CreateAuctionCommandValidatorTests
{
    private readonly CreateAuctionCommandValidator _validator = new();

    private static CreateAuctionCommand ValidCommand() => new()
    {
        PropertyId = Guid.NewGuid(),
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(7),
        StartingPrice = 100_000,
        CommissionPercentage = 2.5m,
        VatPercentage = 15
    };

    [Fact]
    public void Should_Fail_When_EndDate_Not_After_StartDate()
    {
        var command = ValidCommand() with { EndDate = ValidCommand().StartDate.AddDays(-1) };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAuctionCommand.EndDate));
    }

    [Fact]
    public void Should_Fail_When_StartingPrice_Is_Zero()
    {
        var command = ValidCommand() with { StartingPrice = 0 };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAuctionCommand.StartingPrice));
    }

    [Fact]
    public void Should_Fail_When_ReservePrice_Below_StartingPrice()
    {
        var command = ValidCommand() with { StartingPrice = 100_000, ReservePrice = 50_000 };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAuctionCommand.ReservePrice));
    }

    [Fact]
    public void Should_Pass_When_ReservePrice_Equals_StartingPrice()
    {
        var command = ValidCommand() with { StartingPrice = 100_000, ReservePrice = 100_000 };

        var result = _validator.Validate(command);

        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Should_Validate_CommissionPercentage_Range(decimal percentage, bool expectedValid)
    {
        var command = ValidCommand() with { CommissionPercentage = percentage };

        var result = _validator.Validate(command);

        result.Errors.Any(e => e.PropertyName == nameof(CreateAuctionCommand.CommissionPercentage)).Should().Be(!expectedValid);
    }
}
