using FalakAlkhair.Application.Agreements.Commands.ApproveAgreement;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class ApproveAgreementCommandHandlerTests
{
    [Fact]
    public async Task Should_Activate_Draft_Agreement()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var agreement = new PropertyManagementAgreement
        {
            CompanyId = currentUser.CompanyId!.Value,
            ContractNumber = "PMA-000001",
            OwnerId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            Status = ManagementAgreementStatus.Draft
        };
        context.PropertyManagementAgreements.Add(agreement);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ApproveAgreementCommandHandler(context, currentUser);
        await handler.Handle(new ApproveAgreementCommand(agreement.Id), CancellationToken.None);

        var updated = await context.PropertyManagementAgreements.FindAsync(agreement.Id);
        updated!.Status.Should().Be(ManagementAgreementStatus.Active);
        updated.ApprovedByUserId.Should().Be(currentUser.UserId);
    }

    [Fact]
    public async Task Should_Reject_Approving_Already_Terminated_Agreement()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var agreement = new PropertyManagementAgreement
        {
            CompanyId = currentUser.CompanyId!.Value,
            ContractNumber = "PMA-000002",
            OwnerId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddYears(-1),
            EndDate = DateTime.UtcNow.AddMonths(-1),
            Status = ManagementAgreementStatus.Terminated
        };
        context.PropertyManagementAgreements.Add(agreement);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ApproveAgreementCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(new ApproveAgreementCommand(agreement.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
