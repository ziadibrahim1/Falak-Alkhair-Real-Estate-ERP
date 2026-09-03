using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceQuotations.Commands.CreateQuotation;

public record CreateQuotationItemDto
{
    public string Description { get; init; } = default!;
    public decimal Quantity { get; init; } = 1;
    public decimal UnitPrice { get; init; }
}

public record CreateQuotationCommand : IRequest<Guid>
{
    public Guid VendorId { get; init; }
    public Guid MaintenanceRequestId { get; init; }
    public DateTime? ValidUntil { get; init; }
    public decimal VatPercentage { get; init; } = 15;
    public string? Notes { get; init; }
    public List<CreateQuotationItemDto> Items { get; init; } = new();
}

public class CreateQuotationCommandValidator : AbstractValidator<CreateQuotationCommand>
{
    public CreateQuotationCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.MaintenanceRequestId).NotEmpty();
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Items).NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل لعرض السعر.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(300);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateQuotationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var maintenanceRequestExists = await _context.MaintenanceRequests.AnyAsync(
            r => r.Id == request.MaintenanceRequestId && r.CompanyId == companyId && !r.IsDeleted, cancellationToken);
        if (!maintenanceRequestExists) throw new NotFoundException(nameof(Domain.Entities.MaintenanceRequest), request.MaintenanceRequestId);

        var vendorExists = await _context.Vendors.AnyAsync(
            v => v.Id == request.VendorId && v.CompanyId == companyId && !v.IsDeleted, cancellationToken);
        if (!vendorExists) throw new NotFoundException(nameof(Domain.Entities.Vendor), request.VendorId);

        var code = await _numberGenerator.GenerateNextNumberAsync("QUOT", companyId, cancellationToken);

        var quotation = new MaintenanceQuotation
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            QuotationNumber = code,
            VendorId = request.VendorId,
            MaintenanceRequestId = request.MaintenanceRequestId,
            ValidUntil = request.ValidUntil,
            VatPercentage = request.VatPercentage,
            Status = QuotationStatus.Pending,
            Notes = request.Notes
        };

        decimal subtotal = 0;
        foreach (var itemRequest in request.Items)
        {
            var lineTotal = Math.Round(itemRequest.Quantity * itemRequest.UnitPrice, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;

            quotation.Items.Add(new MaintenanceQuotationItem
            {
                Description = itemRequest.Description,
                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                LineTotal = lineTotal
            });
        }

        var vatAmount = Math.Round(subtotal * request.VatPercentage / 100, 2, MidpointRounding.AwayFromZero);
        quotation.SubtotalAmount = subtotal;
        quotation.VatAmount = vatAmount;
        quotation.TotalAmount = subtotal + vatAmount;

        _context.MaintenanceQuotations.Add(quotation);
        await _context.SaveChangesAsync(cancellationToken);

        return quotation.Id;
    }
}
