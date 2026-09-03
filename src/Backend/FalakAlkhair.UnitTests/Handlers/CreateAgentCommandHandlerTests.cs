using FalakAlkhair.Application.Agents.Commands.CreateAgent;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class CreateAgentCommandHandlerTests
{
    [Fact]
    public async Task Should_Create_Agent_With_Generated_Code_And_Active_Status()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var handler = new CreateAgentCommandHandler(context, currentUser, new FakeNumberGeneratorService());

        var command = new CreateAgentCommand
        {
            NameAr = "سعود المسوّق",
            Mobile = "0555555555",
            FalLicenseNumber = "FAL-12345",
            DefaultCommissionPercentage = 2.5m
        };

        var agentId = await handler.Handle(command, CancellationToken.None);

        var agent = await context.Agents.FindAsync(agentId);
        agent.Should().NotBeNull();
        agent!.AgentCode.Should().Be("AGENT-000001");
        agent.Status.Should().Be(AgentStatus.Active);
        agent.IsActive.Should().BeTrue();
        agent.CompanyId.Should().Be(currentUser.CompanyId!.Value);
    }
}
