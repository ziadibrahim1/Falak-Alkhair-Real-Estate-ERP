using System.Text;
using FalakAlkhair.Application.Documents.Commands.UploadDocument;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class UploadDocumentCommandHandlerTests
{
    [Fact]
    public async Task Should_Save_File_And_Persist_Document_Row()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var fileStorage = new FakeFileStorageService();

        var handler = new UploadDocumentCommandHandler(context, currentUser, fileStorage);
        var entityId = Guid.NewGuid();

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("dummy pdf content"));
        var id = await handler.Handle(new UploadDocumentCommand
        {
            FileContent = content,
            FileName = "deed.pdf",
            MimeType = "application/pdf",
            FileSize = content.Length,
            DocumentType = "صك ملكية",
            EntityType = "Property",
            EntityId = entityId
        }, CancellationToken.None);

        var document = await context.Documents.FindAsync(id);
        document.Should().NotBeNull();
        document!.EntityType.Should().Be("Property");
        document.EntityId.Should().Be(entityId);
        document.FileName.Should().Be("deed.pdf");
        document.FilePath.Should().NotBeNullOrEmpty();

        // التأكد أن الملف فعليًا محفوظ في التخزين الوهمي بنفس المسار المسجَّل في السجل.
        var savedStream = await fileStorage.OpenReadAsync(document.FilePath, CancellationToken.None);
        using var reader = new StreamReader(savedStream);
        (await reader.ReadToEndAsync()).Should().Be("dummy pdf content");
    }
}
