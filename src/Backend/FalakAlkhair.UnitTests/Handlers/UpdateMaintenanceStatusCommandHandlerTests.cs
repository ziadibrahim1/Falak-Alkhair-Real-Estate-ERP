using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.MaintenanceRequests.Commands.UpdateMaintenanceStatus;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class UpdateMaintenanceStatusCommandHandlerTests
{
    [Fact]
    public async Task Should_Reject_Setting_Status_To_Approved_Directly()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var request = new MaintenanceRequest
        {
            CompanyId = currentUser.CompanyId!.Value,
            RequestNumber = "MAINT-000001",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            RequestType = MaintenanceRequestType.Plumbing,
            Description = "تسريب مياه",
            Status = MaintenanceStatus.Quotation
        };
        context.MaintenanceRequests.Add(request);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateMaintenanceStatusCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(
            new UpdateMaintenanceStatusCommand { Id = request.Id, Status = MaintenanceStatus.Approved }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Set_CompletionDate_And_ActualCost_When_Completed()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var request = new MaintenanceRequest
        {
            CompanyId = currentUser.CompanyId!.Value,
            RequestNumber = "MAINT-000002",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            RequestType = MaintenanceRequestType.Electrical,
            Description = "عطل كهربائي",
            Status = MaintenanceStatus.InProgress
        };
        context.MaintenanceRequests.Add(request);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateMaintenanceStatusCommandHandler(context, currentUser);
        await handler.Handle(
            new UpdateMaintenanceStatusCommand { Id = request.Id, Status = MaintenanceStatus.Completed, ActualCost = 350 },
            CancellationToken.None);

        var updated = await context.MaintenanceRequests.FindAsync(request.Id);
        updated!.Status.Should().Be(MaintenanceStatus.Completed);
        updated.CompletionDate.Should().NotBeNull();
        updated.ActualCost.Should().Be(350);
    }
}
