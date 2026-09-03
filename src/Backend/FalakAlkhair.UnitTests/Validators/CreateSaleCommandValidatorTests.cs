using FalakAlkhair.Application.Sales.Commands.CreateSale;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class CreateSaleCommandValidatorTests
{
    private readonly CreateSaleCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_FinalPrice_Is_Zero()
    {
        var command = new CreateSaleCommand
        {
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            FinalPrice = 0,
            CommissionPercentage = 2.5m
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSaleCommand.FinalPrice));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Should_Validate_Commission_Percentage_Range(decimal percentage, bool expectedValid)
    {
        var command = new CreateSaleCommand
        {
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            FinalPrice = 500_000,
            CommissionPercentage = percentage
        };

        var result = _validator.Validate(command);

        result.Errors.Any(e => e.PropertyName == nameof(CreateSaleCommand.CommissionPercentage)).Should().Be(!expectedValid);
    }
}
