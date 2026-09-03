using FalakAlkhair.Application.Documents.Commands.UploadDocument;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Validators;

public class UploadDocumentCommandValidatorTests
{
    private readonly UploadDocumentCommandValidator _validator = new();

    private static UploadDocumentCommand ValidCommand() => new()
    {
        FileContent = Stream.Null,
        FileName = "deed.pdf",
        MimeType = "application/pdf",
        FileSize = 1024,
        DocumentType = "صك ملكية",
        EntityType = "Property",
        EntityId = Guid.NewGuid()
    };

    [Fact]
    public void Should_Fail_When_Extension_Not_Allowed()
    {
        var command = ValidCommand() with { FileName = "malware.exe" };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadDocumentCommand.FileName));
    }

    [Fact]
    public void Should_Pass_For_Allowed_Extension()
    {
        var command = ValidCommand() with { FileName = "photo.png" };

        var result = _validator.Validate(command);

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Should_Fail_When_FileSize_Exceeds_Limit()
    {
        var command = ValidCommand() with { FileSize = 21 * 1024 * 1024 };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadDocumentCommand.FileSize));
    }

    [Fact]
    public void Should_Fail_When_EntityId_Empty()
    {
        var command = ValidCommand() with { EntityId = Guid.Empty };

        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadDocumentCommand.EntityId));
    }
}
