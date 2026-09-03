using FalakAlkhair.Application.Auctions.Commands.ProcessAuctionWebhookEvent;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class ProcessAuctionWebhookEventCommandHandlerTests
{
    private static Auction BuildPublishedAuction(Guid companyId, string externalId)
        => new()
        {
            CompanyId = companyId,
            AuctionNumber = "AUCT-000020",
            PropertyId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(6),
            StartingPrice = 100_000,
            CommissionPercentage = 2,
            VatPercentage = 15,
            Status = AuctionStatus.Published,
            ExternalAuctionId = externalId
        };

    [Fact]
    public async Task Should_Throw_When_ExternalAuctionId_Unknown()
    {
        await using var context = TestDbContext.Create();
        var handler = new ProcessAuctionWebhookEventCommandHandler(context);

        var act = async () => await handler.Handle(
            new ProcessAuctionWebhookEventCommand { ExternalAuctionId = "does-not-exist", EventType = nameof(AuctionEventType.BidPlaced) },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Should_Update_CurrentBid_And_BidsCount_On_BidPlaced()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildPublishedAuction(currentUser.CompanyId!.Value, "ext-123");
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ProcessAuctionWebhookEventCommandHandler(context);
        await handler.Handle(
            new ProcessAuctionWebhookEventCommand
            {
                ExternalAuctionId = "ext-123",
                EventType = nameof(AuctionEventType.BidPlaced),
                BidAmount = 110_000
            }, CancellationToken.None);

        var updated = await context.Auctions.FindAsync(auction.Id);
        updated!.CurrentBidAmount.Should().Be(110_000);
        updated.BidsCount.Should().Be(1);

        var auditLog = context.AuctionAuditLogs.Single(l => l.AuctionId == auction.Id);
        auditLog.EventType.Should().Be(AuctionEventType.BidPlaced);
    }

    [Fact]
    public async Task Should_Transition_Published_To_Live_On_AuctionWentLive()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildPublishedAuction(currentUser.CompanyId!.Value, "ext-456");
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ProcessAuctionWebhookEventCommandHandler(context);
        await handler.Handle(
            new ProcessAuctionWebhookEventCommand { ExternalAuctionId = "ext-456", EventType = nameof(AuctionEventType.AuctionWentLive) },
            CancellationToken.None);

        var updated = await context.Auctions.FindAsync(auction.Id);
        updated!.Status.Should().Be(AuctionStatus.Live);
    }

    [Fact]
    public async Task Should_Always_Record_Audit_Log_Regardless_Of_Event_Type()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildPublishedAuction(currentUser.CompanyId!.Value, "ext-789");
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ProcessAuctionWebhookEventCommandHandler(context);
        await handler.Handle(
            new ProcessAuctionWebhookEventCommand
            {
                ExternalAuctionId = "ext-789",
                EventType = nameof(AuctionEventType.PaymentReceived),
                RawPayload = "{\"amount\":1000}",
                SourceIp = "10.0.0.1"
            }, CancellationToken.None);

        var auditLog = context.AuctionAuditLogs.Single(l => l.AuctionId == auction.Id);
        auditLog.EventType.Should().Be(AuctionEventType.PaymentReceived);
        auditLog.Payload.Should().Be("{\"amount\":1000}");
        auditLog.SourceIp.Should().Be("10.0.0.1");
    }
}
