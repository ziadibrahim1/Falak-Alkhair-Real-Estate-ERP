using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Sales.Commands.UpdateSaleStage;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class UpdateSaleStageCommandHandlerTests
{
    [Fact]
    public async Task Should_Generate_Commission_And_Mark_Unit_Sold_When_Stage_Reaches_Completed()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var agentId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        context.Units.Add(new Unit
        {
            Id = unitId, CompanyId = companyId, PropertyId = Guid.NewGuid(), UnitCode = "UNIT-000001",
            UnitNumber = "1", UnitType = UnitType.Apartment, CurrentStatus = UnitStatus.ListedForSale
        });

        var sale = new Sale
        {
            CompanyId = companyId,
            SaleNumber = "SALE-000001",
            PropertyId = Guid.NewGuid(),
            UnitId = unitId,
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            AgentId = agentId,
            AskingPrice = 1_000_000,
            FinalPrice = 950_000,
            CommissionPercentage = 2.5m,
            Stage = SaleStage.Contract
        };
        context.Sales.Add(sale);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateSaleStageCommandHandler(context, currentUser, new FakeNumberGeneratorService(), new FakeNotificationService(context));
        await handler.Handle(new UpdateSaleStageCommand { Id = sale.Id, Stage = SaleStage.Completed }, CancellationToken.None);

        var updatedUnit = await context.Units.FindAsync(unitId);
        updatedUnit!.CurrentStatus.Should().Be(UnitStatus.Sold);

        var commission = context.Commissions.Single(c => c.SaleId == sale.Id);
        commission.AgentId.Should().Be(agentId);
        commission.CommissionAmount.Should().Be(23750); // 950,000 * 2.5%
        commission.NetCommissionAmount.Should().Be(23750 + 3562.5m); // + 15% VAT
    }

    [Fact]
    public async Task Should_Reject_Moving_Backward_In_Pipeline()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var sale = new Sale
        {
            CompanyId = currentUser.CompanyId!.Value,
            SaleNumber = "SALE-000002",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            AskingPrice = 500_000,
            FinalPrice = 500_000,
            Stage = SaleStage.Negotiation
        };
        context.Sales.Add(sale);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateSaleStageCommandHandler(context, currentUser, new FakeNumberGeneratorService(), new FakeNotificationService(context));

        var act = async () => await handler.Handle(
            new UpdateSaleStageCommand { Id = sale.Id, Stage = SaleStage.Qualified }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
