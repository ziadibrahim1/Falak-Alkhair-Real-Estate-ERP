using FalakAlkhair.Application.MaintenanceQuotations.Commands.ApproveQuotation;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class ApproveQuotationCommandHandlerTests
{
    [Fact]
    public async Task Should_Reject_Sibling_Quotations_And_Update_Request_When_Approved()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var companyId = currentUser.CompanyId!.Value;
        var vendorId = Guid.NewGuid();

        var maintenanceRequest = new MaintenanceRequest
        {
            CompanyId = companyId,
            RequestNumber = "MAINT-000001",
            PropertyId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            RequestType = MaintenanceRequestType.AC,
            Description = "تعطل مكيف",
            Status = MaintenanceStatus.Quotation
        };
        context.MaintenanceRequests.Add(maintenanceRequest);

        var quotationToApprove = new MaintenanceQuotation
        {
            CompanyId = companyId,
            QuotationNumber = "QUOT-000001",
            VendorId = vendorId,
            MaintenanceRequestId = maintenanceRequest.Id,
            SubtotalAmount = 1000,
            VatAmount = 150,
            TotalAmount = 1150,
            Status = QuotationStatus.Pending
        };
        var siblingQuotation = new MaintenanceQuotation
        {
            CompanyId = companyId,
            QuotationNumber = "QUOT-000002",
            VendorId = Guid.NewGuid(),
            MaintenanceRequestId = maintenanceRequest.Id,
            SubtotalAmount = 1200,
            VatAmount = 180,
            TotalAmount = 1380,
            Status = QuotationStatus.Pending
        };
        context.MaintenanceQuotations.AddRange(quotationToApprove, siblingQuotation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ApproveQuotationCommandHandler(context, currentUser);
        await handler.Handle(new ApproveQuotationCommand(quotationToApprove.Id), CancellationToken.None);

        (await context.MaintenanceQuotations.FindAsync(quotationToApprove.Id))!.Status.Should().Be(QuotationStatus.Approved);
        (await context.MaintenanceQuotations.FindAsync(siblingQuotation.Id))!.Status.Should().Be(QuotationStatus.Rejected);

        var updatedRequest = await context.MaintenanceRequests.FindAsync(maintenanceRequest.Id);
        updatedRequest!.Status.Should().Be(MaintenanceStatus.Approved);
        updatedRequest.EstimatedCost.Should().Be(1150);
        updatedRequest.AssignedVendorId.Should().Be(vendorId);
    }
}
