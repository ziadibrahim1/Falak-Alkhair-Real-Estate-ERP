using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Commissions.Commands.MarkCommissionPaid;

/// <summary>تسجيل صرف عمولة للمسوّق: Pending/Approved → Paid.</summary>
public record MarkCommissionPaidCommand(Guid Id) : IRequest;

public class MarkCommissionPaidCommandHandler : IRequestHandler<MarkCommissionPaidCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkCommissionPaidCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkCommissionPaidCommand request, CancellationToken cancellationToken)
    {
        var commission = await _context.Commissions
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Commission), request.Id);

        if (commission.Status == CommissionStatus.Cancelled)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن صرف عمولة ملغاة.");
        }

        if (commission.Status == CommissionStatus.Paid)
        {
            throw new Common.Exceptions.BusinessRuleException("تم صرف هذه العمولة بالفعل.");
        }

        commission.Status = CommissionStatus.Paid;
        commission.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
