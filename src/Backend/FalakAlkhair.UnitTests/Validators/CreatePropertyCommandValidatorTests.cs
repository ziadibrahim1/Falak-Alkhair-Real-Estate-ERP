using FalakAlkhair.Application.Properties.Commands.CreateProperty;
using FalakAlkhair.Domain.Common.Enums;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class CreatePropertyCommandValidatorTests
{
    private readonly CreatePropertyCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_OwnerId_Is_Empty()
    {
        var command = new CreatePropertyCommand
        {
            PropertyName = "عقار تجريبي",
            OwnerId = Guid.Empty,
            PropertyType = PropertyType.Villa,
            PropertyCategory = PropertyCategory.Residential
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePropertyCommand.OwnerId));
    }

    [Fact]
    public void Should_Fail_When_TotalArea_Is_Not_Positive()
    {
        var command = new CreatePropertyCommand
        {
            PropertyName = "عقار",
            OwnerId = Guid.NewGuid(),
            PropertyType = PropertyType.Land,
            PropertyCategory = PropertyCategory.Land,
            TotalArea = -10
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePropertyCommand.TotalArea));
    }

    [Fact]
    public void Should_Pass_With_Valid_Minimal_Data()
    {
        var command = new CreatePropertyCommand
        {
            PropertyName = "فيلا حي النرجس",
            OwnerId = Guid.NewGuid(),
            PropertyType = PropertyType.Villa,
            PropertyCategory = PropertyCategory.Residential
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
