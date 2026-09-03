using FalakAlkhair.Application.Auctions.Commands.AwardAuction;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class AwardAuctionCommandHandlerTests
{
    private static Auction BuildAuction(Guid companyId, Guid? agentId = null, decimal commissionPercentage = 2.5m, decimal? reservePrice = null)
        => new()
        {
            CompanyId = companyId,
            AuctionNumber = "AUCT-000001",
            PropertyId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            AgentId = agentId,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(-1),
            StartingPrice = 100_000,
            ReservePrice = reservePrice,
            CommissionPercentage = commissionPercentage,
            VatPercentage = 15,
            Status = AuctionStatus.Ended
        };

    [Fact]
    public async Task Should_Reject_When_Auction_Not_Ended()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value);
        auction.Status = AuctionStatus.Live;
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AwardAuctionCommandHandler(context, currentUser, new FakeNumberGeneratorService());

        var act = async () => await handler.Handle(
            new AwardAuctionCommand { Id = auction.Id, WinnerBuyerId = Guid.NewGuid(), FinalPrice = 150_000 },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Reject_When_FinalPrice_Below_ReservePrice()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, reservePrice: 120_000);
        context.Auctions.Add(auction);

        var buyer = new Buyer { CompanyId = currentUser.CompanyId!.Value, BuyerCode = "BUYER-000001", NameAr = "مشترٍ تجريبي", Mobile = "0500000001" };
        context.Buyers.Add(buyer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AwardAuctionCommandHandler(context, currentUser, new FakeNumberGeneratorService());

        var act = async () => await handler.Handle(
            new AwardAuctionCommand { Id = auction.Id, WinnerBuyerId = buyer.Id, FinalPrice = 100_000 },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Should_Award_And_Generate_Commission_With_Vat()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var agentId = Guid.NewGuid();
        var auction = BuildAuction(currentUser.CompanyId!.Value, agentId: agentId, commissionPercentage: 2m);
        context.Auctions.Add(auction);

        var buyer = new Buyer { CompanyId = currentUser.CompanyId!.Value, BuyerCode = "BUYER-000002", NameAr = "مشترٍ فائز", Mobile = "0500000002" };
        context.Buyers.Add(buyer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AwardAuctionCommandHandler(context, currentUser, new FakeNumberGeneratorService());
        await handler.Handle(
            new AwardAuctionCommand { Id = auction.Id, WinnerBuyerId = buyer.Id, FinalPrice = 200_000 },
            CancellationToken.None);

        var updated = await context.Auctions.FindAsync(auction.Id);
        updated!.Status.Should().Be(AuctionStatus.Awarded);
        updated.WinnerBuyerId.Should().Be(buyer.Id);
        updated.FinalPrice.Should().Be(200_000);

        var commission = context.Commissions.Single(c => c.AuctionId == auction.Id);
        commission.SourceType.Should().Be(CommissionSourceType.Auction);
        commission.BaseAmount.Should().Be(200_000);
        commission.CommissionAmount.Should().Be(4_000);
        commission.VatAmount.Should().Be(600);
        commission.NetCommissionAmount.Should().Be(4_600);

        var auditLog = context.AuctionAuditLogs.Single(l => l.AuctionId == auction.Id);
        auditLog.EventType.Should().Be(AuctionEventType.AuctionAwarded);
    }

    [Fact]
    public async Task Should_Not_Generate_Commission_When_No_Agent()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var auction = BuildAuction(currentUser.CompanyId!.Value, agentId: null);
        context.Auctions.Add(auction);

        var buyer = new Buyer { CompanyId = currentUser.CompanyId!.Value, BuyerCode = "BUYER-000003", NameAr = "مشترٍ آخر", Mobile = "0500000003" };
        context.Buyers.Add(buyer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AwardAuctionCommandHandler(context, currentUser, new FakeNumberGeneratorService());
        await handler.Handle(
            new AwardAuctionCommand { Id = auction.Id, WinnerBuyerId = buyer.Id, FinalPrice = 150_000 },
            CancellationToken.None);

        context.Commissions.Any(c => c.AuctionId == auction.Id).Should().BeFalse();
    }
}
