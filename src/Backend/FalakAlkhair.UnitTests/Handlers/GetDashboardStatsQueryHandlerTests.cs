using FalakAlkhair.Application.Dashboard.Queries.GetDashboardStats;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class GetDashboardStatsQueryHandlerTests
{
    [Fact]
    public async Task Should_Aggregate_Stats_Correctly_Scoped_To_Current_Company()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var otherCompanyId = Guid.NewGuid();

        context.Units.AddRange(
            new Unit { CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "U1", UnitNumber = "1", CurrentStatus = UnitStatus.Available },
            new Unit { CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "U2", UnitNumber = "2", CurrentStatus = UnitStatus.Rented },
            new Unit { CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "U3", UnitNumber = "3", CurrentStatus = UnitStatus.Sold },
            // وحدة تابعة لشركة أخرى — يجب ألا تُحتسَب.
            new Unit { CompanyId = otherCompanyId, PropertyId = Guid.NewGuid(), UnitCode = "U4", UnitNumber = "4", CurrentStatus = UnitStatus.Available });

        context.Leases.Add(new Lease
        {
            CompanyId = companyId, TenantId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), PropertyId = Guid.NewGuid(), UnitId = Guid.NewGuid(),
            LeaseNumber = "L1", StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(11),
            AnnualRentAmount = 40000, Status = LeaseStatus.Active
        });

        context.MaintenanceRequests.Add(new MaintenanceRequest
        {
            CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitId = Guid.NewGuid(), OwnerId = Guid.NewGuid(),
            RequestNumber = "M1", RequestType = MaintenanceRequestType.AC, Priority = MaintenancePriority.Critical,
            Description = "عطل", Status = MaintenanceStatus.New
        });

        context.Auctions.Add(new Auction
        {
            CompanyId = companyId, PropertyId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), AuctionNumber = "A1",
            StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(8), StartingPrice = 100000,
            CommissionPercentage = 2, VatPercentage = 15, Status = AuctionStatus.Live
        });

        context.Commissions.Add(new Commission
        {
            CompanyId = companyId, AgentId = Guid.NewGuid(), CommissionNumber = "C1", SourceType = CommissionSourceType.Lease,
            BaseAmount = 40000, CommissionPercentage = 5, CommissionAmount = 2000, VatPercentage = 15, VatAmount = 300,
            NetCommissionAmount = 2300, Status = CommissionStatus.Pending
        });

        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardStatsQueryHandler(context, currentUser);
        var stats = await handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        stats.TotalUnits.Should().Be(3);
        stats.AvailableUnits.Should().Be(1);
        stats.RentedUnits.Should().Be(1);
        stats.SoldUnits.Should().Be(1);
        stats.ActiveLeases.Should().Be(1);
        stats.ActiveLeasesAnnualRentValue.Should().Be(40000);
        stats.OpenMaintenanceRequests.Should().Be(1);
        stats.UrgentMaintenanceRequests.Should().Be(1);
        stats.LiveAuctions.Should().Be(1);
        stats.PendingCommissionsAmount.Should().Be(2300);
    }
}
