using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceQuotations.Commands.ApproveQuotation;

/// <summary>
/// اعتماد عرض سعر صيانة: يرفض تلقائيًا بقية العروض المعلَّقة على نفس الطلب
/// (لدعم مقارنة أكثر من عرض)، ويحدّث طلب الصيانة (التكلفة التقديرية، المورّد
/// المسند، الحالة → Approved).
/// </summary>
public record ApproveQuotationCommand(Guid Id) : IRequest;

public class ApproveQuotationCommandHandler : IRequestHandler<ApproveQuotationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public ApproveQuotationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task Handle(ApproveQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _context.MaintenanceQuotations
            .Where(q => q.CompanyId == _currentUser.CompanyId && !q.IsDeleted)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceQuotation), request.Id);

        if (quotation.Status != QuotationStatus.Pending)
        {
            throw new Common.Exceptions.BusinessRuleException("تم البت في هذا العرض بالفعل.");
        }

        var maintenanceRequest = await _context.MaintenanceRequests
            .FirstAsync(r => r.Id == quotation.MaintenanceRequestId, cancellationToken);

        if (maintenanceRequest.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن اعتماد عرض لطلب بحالته الحالية ({maintenanceRequest.Status}).");
        }

        var siblingQuotations = await _context.MaintenanceQuotations
            .Where(q => q.MaintenanceRequestId == quotation.MaintenanceRequestId && q.Id != quotation.Id && q.Status == QuotationStatus.Pending)
            .ToListAsync(cancellationToken);
        foreach (var sibling in siblingQuotations)
        {
            sibling.Status = QuotationStatus.Rejected;
        }

        quotation.Status = QuotationStatus.Approved;

        maintenanceRequest.EstimatedCost = quotation.TotalAmount;
        maintenanceRequest.AssignedVendorId = quotation.VendorId;
        maintenanceRequest.Status = MaintenanceStatus.Approved;

        _notifications.Notify(
            quotation.CompanyId,
            quotation.BranchId,
            userId: null,
            Domain.Common.Enums.NotificationType.QuotationApproved,
            "تم اعتماد عرض سعر صيانة",
            $"تم اعتماد عرض السعر \"{quotation.QuotationNumber}\" بإجمالي {quotation.TotalAmount:N2}.",
            link: "/quotations");

        await _context.SaveChangesAsync(cancellationToken);
    }
}
