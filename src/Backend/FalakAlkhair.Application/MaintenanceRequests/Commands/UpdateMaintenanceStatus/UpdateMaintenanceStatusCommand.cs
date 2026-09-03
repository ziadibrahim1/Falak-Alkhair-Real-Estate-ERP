using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceRequests.Commands.UpdateMaintenanceStatus;

/// <summary>
/// تحديث حالة طلب الصيانة يدويًا ضمن دورة العمل (تقدُّم للأمام فقط، عدا الإلغاء
/// المسموح من أي حالة غير نهائية). حالة Approved لا تُضبَط هنا — تُضبَط فقط
/// تلقائيًا عبر اعتماد عرض سعر (راجع ApproveQuotationCommand).
/// </summary>
public record UpdateMaintenanceStatusCommand : IRequest
{
    public Guid Id { get; init; }
    public MaintenanceStatus Status { get; init; }
    public decimal? ActualCost { get; init; }
}

public class UpdateMaintenanceStatusCommandValidator : AbstractValidator<UpdateMaintenanceStatusCommand>
{
    public UpdateMaintenanceStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class UpdateMaintenanceStatusCommandHandler : IRequestHandler<UpdateMaintenanceStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateMaintenanceStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMaintenanceStatusCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .Where(r => r.CompanyId == _currentUser.CompanyId && !r.IsDeleted)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceRequest), request.Id);

        if (maintenanceRequest.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن تعديل طلب بحالته الحالية ({maintenanceRequest.Status}).");
        }

        if (request.Status == MaintenanceStatus.Approved)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن ضبط حالة \"معتمد\" مباشرة — اعتمد عرض سعر بدلًا من ذلك.");
        }

        if (request.Status != MaintenanceStatus.Cancelled && request.Status <= maintenanceRequest.Status)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن الرجوع لحالة سابقة أو نفس الحالة الحالية.");
        }

        maintenanceRequest.Status = request.Status;

        if (request.Status == MaintenanceStatus.InProgress && maintenanceRequest.StartDate is null)
        {
            maintenanceRequest.StartDate = DateTime.UtcNow;
        }

        if (request.Status == MaintenanceStatus.Completed)
        {
            maintenanceRequest.CompletionDate = DateTime.UtcNow;
            if (request.ActualCost.HasValue)
            {
                maintenanceRequest.ActualCost = request.ActualCost;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
