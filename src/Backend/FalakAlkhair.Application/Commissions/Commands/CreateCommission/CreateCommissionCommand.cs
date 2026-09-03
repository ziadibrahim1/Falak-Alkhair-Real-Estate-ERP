using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Commissions.Commands.CreateCommission;

/// <summary>
/// تسجيل عمولة يدويًا لمسوّق (مثال: عمولة استثنائية أو تعديل لا يرتبط بتفعيل
/// عقد إيجار مباشرة). العمولات الناتجة عن تفعيل عقود الإيجار تُولَّد تلقائيًا
/// (راجع ActivateLeaseCommand) ولا تحتاج هذا الأمر.
/// </summary>
public record CreateCommissionCommand : IRequest<Guid>
{
    public Guid AgentId { get; init; }
    public CommissionSourceType SourceType { get; init; }
    public Guid? LeaseId { get; init; }
    public decimal BaseAmount { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal VatPercentage { get; init; } = 15;
    public string? Notes { get; init; }
}

public class CreateCommissionCommandValidator : AbstractValidator<CreateCommissionCommand>
{
    public CreateCommissionCommandValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.BaseAmount).GreaterThan(0).WithMessage("المبلغ الأساسي يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.LeaseId).NotEmpty().When(x => x.SourceType == CommissionSourceType.Lease)
            .WithMessage("عقد الإيجار مطلوب عندما يكون مصدر العمولة إيجارًا.");
    }
}

public class CreateCommissionCommandHandler : IRequestHandler<CreateCommissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateCommissionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateCommissionCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId && a.CompanyId == companyId && !a.IsDeleted, cancellationToken);
        if (!agentExists) throw new NotFoundException(nameof(Agent), request.AgentId);

        if (request.LeaseId.HasValue)
        {
            var leaseExists = await _context.Leases.AnyAsync(l => l.Id == request.LeaseId.Value && l.CompanyId == companyId && !l.IsDeleted, cancellationToken);
            if (!leaseExists) throw new NotFoundException(nameof(Lease), request.LeaseId.Value);
        }

        var commissionAmount = Math.Round(request.BaseAmount * request.CommissionPercentage / 100, 2, MidpointRounding.AwayFromZero);
        var vatAmount = Math.Round(commissionAmount * request.VatPercentage / 100, 2, MidpointRounding.AwayFromZero);

        var number = await _numberGenerator.GenerateNextNumberAsync("COMM", companyId, cancellationToken);

        var commission = new Commission
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            CommissionNumber = number,
            AgentId = request.AgentId,
            SourceType = request.SourceType,
            LeaseId = request.LeaseId,
            BaseAmount = request.BaseAmount,
            CommissionPercentage = request.CommissionPercentage,
            CommissionAmount = commissionAmount,
            VatPercentage = request.VatPercentage,
            VatAmount = vatAmount,
            NetCommissionAmount = commissionAmount + vatAmount,
            Status = CommissionStatus.Pending,
            Notes = request.Notes
        };

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);

        return commission.Id;
    }
}
