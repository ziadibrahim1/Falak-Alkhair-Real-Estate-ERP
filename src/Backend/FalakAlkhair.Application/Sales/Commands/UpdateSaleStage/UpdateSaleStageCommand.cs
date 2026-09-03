using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sales.Commands.UpdateSaleStage;

/// <summary>
/// نقل معاملة بيع للمرحلة التالية ضمن مسار المبيعات (Sales Pipeline). لا يسمح
/// بالتراجع للخلف (إلا الإلغاء من أي مرحلة). عند الوصول لمرحلة Completed
/// تُولَّد عمولة (Commission) تلقائيًا (إن وُجد مسوّق ونسبة عمولة > صفر)
/// وتتحدَّث حالة الوحدة إلى Sold — بنفس فلسفة تفعيل عقد الإيجار.
/// </summary>
public record UpdateSaleStageCommand : IRequest
{
    public Guid Id { get; init; }
    public SaleStage Stage { get; init; }
    public string? CancellationReason { get; init; }
}

public class UpdateSaleStageCommandValidator : AbstractValidator<UpdateSaleStageCommand>
{
    public UpdateSaleStageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CancellationReason).NotEmpty().When(x => x.Stage == SaleStage.Cancelled)
            .WithMessage("سبب الإلغاء مطلوب.");
    }
}

public class UpdateSaleStageCommandHandler : IRequestHandler<UpdateSaleStageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;
    private readonly INotificationService _notifications;

    public UpdateSaleStageCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator, INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
        _notifications = notifications;
    }

    public async Task Handle(UpdateSaleStageCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Sale), request.Id);

        if (sale.Stage is SaleStage.Completed or SaleStage.Cancelled)
        {
            throw new Common.Exceptions.BusinessRuleException($"لا يمكن تعديل معاملة بيع بحالتها الحالية ({sale.Stage}).");
        }

        if (request.Stage != SaleStage.Cancelled && request.Stage <= sale.Stage)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن الرجوع لمرحلة سابقة أو نفس المرحلة الحالية ضمن مسار المبيعات.");
        }

        sale.Stage = request.Stage;

        if (request.Stage == SaleStage.Cancelled)
        {
            sale.CancellationReason = request.CancellationReason;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (request.Stage == SaleStage.Completed)
        {
            sale.CompletedAt = DateTime.UtcNow;

            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == sale.UnitId, cancellationToken);
            if (unit is not null)
            {
                unit.CurrentStatus = UnitStatus.Sold;
            }

            if (sale.AgentId.HasValue && sale.CommissionPercentage > 0)
            {
                var commissionExists = await _context.Commissions.AnyAsync(c => c.SaleId == sale.Id && !c.IsDeleted, cancellationToken);
                if (!commissionExists)
                {
                    const decimal vatPercentage = 15;
                    var commissionAmount = Math.Round(sale.FinalPrice * sale.CommissionPercentage / 100, 2, MidpointRounding.AwayFromZero);
                    var vatAmount = Math.Round(commissionAmount * vatPercentage / 100, 2, MidpointRounding.AwayFromZero);
                    var commissionNumber = await _numberGenerator.GenerateNextNumberAsync("COMM", sale.CompanyId, cancellationToken);

                    _context.Commissions.Add(new Commission
                    {
                        CompanyId = sale.CompanyId,
                        BranchId = sale.BranchId,
                        CommissionNumber = commissionNumber,
                        AgentId = sale.AgentId.Value,
                        SourceType = CommissionSourceType.Sale,
                        SaleId = sale.Id,
                        BaseAmount = sale.FinalPrice,
                        CommissionPercentage = sale.CommissionPercentage,
                        CommissionAmount = commissionAmount,
                        VatPercentage = vatPercentage,
                        VatAmount = vatAmount,
                        NetCommissionAmount = commissionAmount + vatAmount,
                        Status = CommissionStatus.Pending
                    });
                }
            }

            _notifications.Notify(
                sale.CompanyId,
                sale.BranchId,
                userId: null,
                Domain.Common.Enums.NotificationType.SaleCompleted,
                "تم إتمام عملية بيع",
                $"اكتملت عملية البيع \"{sale.SaleNumber}\" بسعر نهائي {sale.FinalPrice:N2}.",
                link: "/sales");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
