using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Commands.UpdateLease;

/// <summary>
/// تعديل بنود عقد الإيجار مسموح فقط طالما كان في حالة Draft (مسودة).
/// بعد التفعيل تصبح بنود العقد المالية ثابتة، وأي تغيير يتطلب إنهاء العقد
/// الحالي وإنشاء عقد جديد (Amendment قابل للإضافة لاحقًا كميزة مستقلة).
/// </summary>
public record UpdateLeaseCommand : IRequest
{
    public Guid Id { get; init; }
    public DateTime EndDate { get; init; }
    public decimal SecurityDeposit { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal VatPercentage { get; init; }
    public string? Notes { get; init; }
}

public class UpdateLeaseCommandValidator : AbstractValidator<UpdateLeaseCommand>
{
    public UpdateLeaseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SecurityDeposit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateLeaseCommandHandler : IRequestHandler<UpdateLeaseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateLeaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lease), request.Id);

        if (lease.Status != LeaseStatus.Draft)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن تعديل عقد بعد تفعيله. أنشئ عقدًا جديدًا أو أنهِ العقد الحالي.");
        }

        if (request.EndDate <= lease.StartDate)
        {
            throw new Common.Exceptions.BusinessRuleException("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        }

        lease.EndDate = request.EndDate;
        lease.SecurityDeposit = request.SecurityDeposit;
        lease.CommissionPercentage = request.CommissionPercentage;
        lease.VatPercentage = request.VatPercentage;
        lease.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
