using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Documents.Commands.DeleteDocument;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class DeleteDocumentCommandHandlerTests
{
    [Fact]
    public async Task Should_SoftDelete_Row_And_Remove_Physical_File()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var fileStorage = new FakeFileStorageService();

        var (relativePath, size) = await fileStorage.SaveAsync(new MemoryStream(new byte[] { 1, 2, 3 }), "id.jpg", "sub", CancellationToken.None);

        var document = new Document
        {
            CompanyId = currentUser.CompanyId!.Value,
            DocumentType = "هوية",
            EntityType = "Owner",
            EntityId = Guid.NewGuid(),
            FileName = "id.jpg",
            FilePath = relativePath,
            FileSize = size,
            MimeType = "image/jpeg"
        };
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteDocumentCommandHandler(context, currentUser, fileStorage);
        await handler.Handle(new DeleteDocumentCommand(document.Id), CancellationToken.None);

        var updated = await context.Documents.FindAsync(document.Id);
        updated!.IsDeleted.Should().BeTrue();

        var act = async () => await fileStorage.OpenReadAsync(relativePath, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
