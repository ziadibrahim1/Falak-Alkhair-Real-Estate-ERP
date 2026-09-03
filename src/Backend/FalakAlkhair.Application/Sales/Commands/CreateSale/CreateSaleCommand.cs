using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sales.Commands.CreateSale;

public record CreateSaleCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid UnitId { get; init; }
    public Guid SellerId { get; init; }
    public Guid BuyerId { get; init; }
    public Guid? AgentId { get; init; }
    public Guid? OfferId { get; init; }
    public decimal AskingPrice { get; init; }
    public decimal FinalPrice { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal VatPercentage { get; init; } = 15;
    public string? Notes { get; init; }
}

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.FinalPrice).GreaterThan(0).WithMessage("السعر النهائي يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
    }
}

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var unit = await _context.Units
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        if (unit.PropertyId != request.PropertyId)
        {
            throw new Common.Exceptions.BusinessRuleException("الوحدة المحددة لا تنتمي للعقار المحدد.");
        }

        var hasActiveSale = await _context.Sales.AnyAsync(
            s => s.UnitId == request.UnitId && !s.IsDeleted && s.Stage != SaleStage.Cancelled && s.Stage != SaleStage.Completed,
            cancellationToken);
        if (hasActiveSale)
        {
            throw new Common.Exceptions.BusinessRuleException("توجد بالفعل معاملة بيع نشطة على هذه الوحدة.");
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("SALE", companyId, cancellationToken);

        var sale = new Sale
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            SaleNumber = code,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            SellerId = request.SellerId,
            BuyerId = request.BuyerId,
            AgentId = request.AgentId,
            OfferId = request.OfferId,
            AskingPrice = request.AskingPrice,
            FinalPrice = request.FinalPrice,
            CommissionPercentage = request.CommissionPercentage,
            VatPercentage = request.VatPercentage,
            Stage = SaleStage.Lead,
            Notes = request.Notes
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);

        return sale.Id;
    }
}
