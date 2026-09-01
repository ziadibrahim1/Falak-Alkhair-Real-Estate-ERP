using FalakAlkhair.Application.Owners.Commands.CreateOwner;
using FalakAlkhair.Domain.Common.Enums;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class CreateOwnerCommandValidatorTests
{
    private readonly CreateOwnerCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_NameAr_Is_Empty()
    {
        var command = new CreateOwnerCommand { NameAr = "", Mobile = "0512345678", PartyType = PartyType.Individual, NationalId = "1012345678" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOwnerCommand.NameAr));
    }

    [Theory]
    [InlineData("0512345678", true)]
    [InlineData("512345678", true)]
    [InlineData("+966512345678", true)]
    [InlineData("123", false)]
    [InlineData("00201234567890", false)]
    public void Should_Validate_Saudi_Mobile_Format(string mobile, bool expectedValid)
    {
        var command = new CreateOwnerCommand
        {
            NameAr = "مالك تجريبي",
            Mobile = mobile,
            PartyType = PartyType.Individual,
            NationalId = "1012345678"
        };

        var result = _validator.Validate(command);

        result.Errors.Any(e => e.PropertyName == nameof(CreateOwnerCommand.Mobile)).Should().Be(!expectedValid);
    }

    [Fact]
    public void Should_Require_NationalId_For_Individual()
    {
        var command = new CreateOwnerCommand
        {
            NameAr = "فرد بدون هوية",
            Mobile = "0512345678",
            PartyType = PartyType.Individual,
            NationalId = null
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOwnerCommand.NationalId));
    }

    [Fact]
    public void Should_Require_CommercialRegistrationNumber_For_Company()
    {
        var command = new CreateOwnerCommand
        {
            NameAr = "شركة بدون سجل",
            Mobile = "0512345678",
            PartyType = PartyType.Company,
            CommercialRegistrationNumber = null
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOwnerCommand.CommercialRegistrationNumber));
    }

    [Fact]
    public void Should_Validate_Iban_Format()
    {
        var command = new CreateOwnerCommand
        {
            NameAr = "مالك",
            Mobile = "0512345678",
            PartyType = PartyType.Individual,
            NationalId = "1012345678",
            Iban = "INVALID"
        };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOwnerCommand.Iban));
    }
}
