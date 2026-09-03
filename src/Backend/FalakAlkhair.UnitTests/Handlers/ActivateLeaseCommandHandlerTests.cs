using FalakAlkhair.Application.Leases.Commands.ActivateLease;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class ActivateLeaseCommandHandlerTests
{
    [Fact]
    public async Task Should_Generate_Commission_When_Lease_Has_Agent_And_Commission_Percentage()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var agentId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        context.Units.Add(new Unit
        {
            Id = unitId, CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "UNIT-000001",
            UnitNumber = "1", UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.Available
        });

        var lease = new Lease
        {
            CompanyId = companyId,
            LeaseNumber = "LEASE-000001",
            TenantId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            UnitId = unitId,
            AgentId = agentId,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            AnnualRentAmount = 40000,
            CommissionPercentage = 5,
            Status = LeaseStatus.Draft
        };
        context.Leases.Add(lease);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ActivateLeaseCommandHandler(context, currentUser, new FakeNumberGeneratorService());
        await handler.Handle(new ActivateLeaseCommand(lease.Id), CancellationToken.None);

        var updatedUnit = await context.Units.FindAsync(unitId);
        updatedUnit!.CurrentStatus.Should().Be(UnitStatus.Rented);

        var commission = context.Commissions.Single(c => c.LeaseId == lease.Id);
        commission.AgentId.Should().Be(agentId);
        commission.CommissionAmount.Should().Be(2000); // 40000 * 5%
        commission.VatAmount.Should().Be(300); // 2000 * 15%
        commission.NetCommissionAmount.Should().Be(2300);
        commission.Status.Should().Be(CommissionStatus.Pending);
    }

    [Fact]
    public async Task Should_Not_Generate_Commission_When_Lease_Has_No_Agent()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var unitId = Guid.NewGuid();

        context.Units.Add(new Unit
        {
            Id = unitId, CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "UNIT-000001",
            UnitNumber = "1", UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.Available
        });

        var lease = new Lease
        {
            CompanyId = companyId,
            LeaseNumber = "LEASE-000002",
            TenantId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            UnitId = unitId,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            AnnualRentAmount = 40000,
            CommissionPercentage = 5,
            Status = LeaseStatus.Draft
        };
        context.Leases.Add(lease);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ActivateLeaseCommandHandler(context, currentUser, new FakeNumberGeneratorService());
        await handler.Handle(new ActivateLeaseCommand(lease.Id), CancellationToken.None);

        context.Commissions.Any(c => c.LeaseId == lease.Id).Should().BeFalse();
    }
}
