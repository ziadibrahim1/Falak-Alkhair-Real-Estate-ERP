using FalakAlkhair.Application.Agents.Commands.CreateAgent;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class CreateAgentCommandValidatorTests
{
    private readonly CreateAgentCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_NameAr_Is_Empty()
    {
        var command = new CreateAgentCommand { NameAr = "", Mobile = "0512345678" };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAgentCommand.NameAr));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(150, false)]
    public void Should_Validate_Commission_Percentage_Range(decimal percentage, bool expectedValid)
    {
        var command = new CreateAgentCommand
        {
            NameAr = "مسوّق تجريبي",
            Mobile = "0512345678",
            DefaultCommissionPercentage = percentage
        };

        var result = _validator.Validate(command);

        result.Errors.Any(e => e.PropertyName == nameof(CreateAgentCommand.DefaultCommissionPercentage)).Should().Be(!expectedValid);
    }
}
