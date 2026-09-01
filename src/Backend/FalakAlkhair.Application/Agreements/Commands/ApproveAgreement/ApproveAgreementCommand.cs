using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agreements.Commands.ApproveAgreement;

/// <summary>
/// اعتماد عقد إدارة الأملاك ينقله من PendingApproval إلى Active وفق الـ Workflow
/// المحدد: Draft → PendingApproval → Active → Expiring → Expired → Terminated.
/// </summary>
public record ApproveAgreementCommand(Guid Id) : IRequest;

public class ApproveAgreementCommandHandler : IRequestHandler<ApproveAgreementCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveAgreementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveAgreementCommand request, CancellationToken cancellationToken)
    {
        var agreement = await _context.PropertyManagementAgreements
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.PropertyManagementAgreement), request.Id);

        if (agreement.Status is not (ManagementAgreementStatus.Draft or ManagementAgreementStatus.PendingApproval))
        {
            throw new Common.Exceptions.BusinessRuleException(
                $"لا يمكن اعتماد عقد بحالته الحالية ({agreement.Status}). يجب أن يكون العقد في حالة مسودة أو بانتظار الاعتماد.");
        }

        agreement.Status = ManagementAgreementStatus.Active;
        agreement.ApprovedAt = DateTime.UtcNow;
        agreement.ApprovedByUserId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
