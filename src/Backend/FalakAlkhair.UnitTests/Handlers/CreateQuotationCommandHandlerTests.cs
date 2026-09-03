using FalakAlkhair.Application.MaintenanceQuotations.Commands.CreateQuotation;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class CreateQuotationCommandHandlerTests
{
    [Fact]
    public async Task Should_Compute_Subtotal_Vat_And_Total_From_Items()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;

        var maintenanceRequest = new MaintenanceRequest
        {
            CompanyId = companyId,
            RequestNumber = "MAINT-000001",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            RequestType = MaintenanceRequestType.AC,
            Description = "صيانة تكييف",
            Status = MaintenanceStatus.Quotation
        };
        var vendor = new Vendor { CompanyId = companyId, VendorCode = "VEND-000001", NameAr = "مورّد", Mobile = "0500000000" };
        context.MaintenanceRequests.Add(maintenanceRequest);
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateQuotationCommandHandler(context, currentUser, new FakeNumberGeneratorService());
        var command = new CreateQuotationCommand
        {
            VendorId = vendor.Id,
            MaintenanceRequestId = maintenanceRequest.Id,
            VatPercentage = 15,
            Items =
            [
                new CreateQuotationItemDto { Description = "قطعة غيار", Quantity = 2, UnitPrice = 100 },
                new CreateQuotationItemDto { Description = "أجرة عمل", Quantity = 1, UnitPrice = 300 }
            ]
        };

        var quotationId = await handler.Handle(command, CancellationToken.None);

        var quotation = await context.MaintenanceQuotations.FindAsync(quotationId);
        quotation!.SubtotalAmount.Should().Be(500); // 2*100 + 1*300
        quotation.VatAmount.Should().Be(75); // 15%
        quotation.TotalAmount.Should().Be(575);
    }
}
