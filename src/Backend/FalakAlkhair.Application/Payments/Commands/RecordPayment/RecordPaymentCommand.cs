using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Payments.Commands.RecordPayment;

/// <summary>
/// تسجيل دفعة تحصيل فعلية على عقد إيجار. إن لم يُحدَّد LeasePaymentId صراحةً،
/// تُطبَّق الدفعة تلقائيًا على أقدم قسط غير مسدد بالكامل (FIFO).
/// </summary>
public record RecordPaymentCommand : IRequest<Guid>
{
    public Guid LeaseId { get; init; }
    public Guid? LeasePaymentId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? BankName { get; init; }
    public string? Notes { get; init; }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.LeaseId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ الدفعة يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.PaymentDate).NotEmpty();
    }
}

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public RecordPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var lease = await _context.Leases
            .Where(l => l.CompanyId == companyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.LeaseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Lease), request.LeaseId);

        LeasePayment? target;
        if (request.LeasePaymentId.HasValue)
        {
            target = await _context.LeasePayments
                .FirstOrDefaultAsync(p => p.Id == request.LeasePaymentId.Value && p.LeaseId == lease.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(LeasePayment), request.LeasePaymentId.Value);
        }
        else
        {
            target = await _context.LeasePayments
                .Where(p => p.LeaseId == lease.Id && p.Status != LeasePaymentStatus.Cancelled && p.PaidAmount < p.Amount)
                .OrderBy(p => p.DueDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (target is null)
        {
            throw new Common.Exceptions.BusinessRuleException("كل الدفعات المستحقة لهذا العقد مسددة بالكامل، لا يوجد قسط لتطبيق الدفعة عليه.");
        }

        var paymentNumber = await _numberGenerator.GenerateNextNumberAsync("PAY", companyId, cancellationToken);

        var payment = new Payment
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            PaymentNumber = paymentNumber,
            LeaseId = lease.Id,
            LeasePaymentId = target.Id,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            BankName = request.BankName,
            Notes = request.Notes
        };

        target.PaidAmount += request.Amount;
        target.Status = target.PaidAmount >= target.Amount ? LeasePaymentStatus.Paid : LeasePaymentStatus.PartiallyPaid;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
