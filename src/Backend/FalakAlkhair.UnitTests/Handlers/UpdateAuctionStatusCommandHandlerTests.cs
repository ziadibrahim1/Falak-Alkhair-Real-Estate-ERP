using FalakAlkhair.Application.Auctions.Commands.UpdateAuctionStatus;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class UpdateAuctionStatusCommandHandlerTests
{
    private static Auction BuildAuction(Guid companyId, AuctionStatus status)
        => new()
        {
            CompanyId = companyId,
            AuctionNumber = "AUCT-000010",
            PropertyId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7),
            StartingPrice = 100_000,
            CommissionPercentage = 2,
            VatPercentage = 15,
            Status = status
        };

    [Fact]
    public async Task Should_Reject_Setting_Restricted_Target_Directly()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, AuctionStatus.Draft);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateAuctionStatusCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(
            new UpdateAuctionStatusCommand { Id = auction.Id, Status = AuctionStatus.Scheduled }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Reject_Moving_Backwards()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, AuctionStatus.Live);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateAuctionStatusCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(
            new UpdateAuctionStatusCommand { Id = auction.Id, Status = AuctionStatus.PendingApproval }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Reject_Modifying_Terminal_Status()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, AuctionStatus.Awarded);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateAuctionStatusCommandHandler(context, currentUser);

        var act = async () => await handler.Handle(
            new UpdateAuctionStatusCommand { Id = auction.Id, Status = AuctionStatus.Cancelled, CancellationReason = "تم الإرساء بالفعل" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Allow_Cancel_From_Live_And_Record_Reason()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, AuctionStatus.Live);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateAuctionStatusCommandHandler(context, currentUser);
        await handler.Handle(
            new UpdateAuctionStatusCommand { Id = auction.Id, Status = AuctionStatus.Cancelled, CancellationReason = "انسحاب المالك" },
            CancellationToken.None);

        var updated = await context.Auctions.FindAsync(auction.Id);
        updated!.Status.Should().Be(AuctionStatus.Cancelled);
        updated.CancellationReason.Should().Be("انسحاب المالك");

        var auditLog = context.AuctionAuditLogs.Single(l => l.AuctionId == auction.Id);
        auditLog.EventType.Should().Be(AuctionEventType.AuctionCancelled);
    }

    [Fact]
    public async Task Should_Allow_Forward_Transition_To_Live()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, AuctionStatus.Published);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateAuctionStatusCommandHandler(context, currentUser);
        await handler.Handle(
            new UpdateAuctionStatusCommand { Id = auction.Id, Status = AuctionStatus.Live }, CancellationToken.None);

        var updated = await context.Auctions.FindAsync(auction.Id);
        updated!.Status.Should().Be(AuctionStatus.Live);

        var auditLog = context.AuctionAuditLogs.Single(l => l.AuctionId == auction.Id);
        auditLog.EventType.Should().Be(AuctionEventType.AuctionWentLive);
    }
}
