using FalakAlkhair.Application.Owners.Commands.CreateOwner;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class CreateOwnerCommandHandlerTests
{
    [Fact]
    public async Task Should_Create_Owner_With_Generated_Code_And_Scoped_To_Current_Company()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var handler = new CreateOwnerCommandHandler(context, currentUser, new FakeNumberGeneratorService());

        var command = new CreateOwnerCommand
        {
            NameAr = "خالد العتيبي",
            Mobile = "0555555555",
            PartyType = PartyType.Individual,
            NationalId = "1099999999"
        };

        var ownerId = await handler.Handle(command, CancellationToken.None);

        var owner = await context.Owners.FindAsync(ownerId);
        owner.Should().NotBeNull();
        owner!.OwnerCode.Should().Be("OWNER-000001");
        owner.CompanyId.Should().Be(currentUser.CompanyId!.Value);
        owner.IsActive.Should().BeTrue();
    }
}
