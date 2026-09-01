using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Units.Commands.CreateUnit;

public record CreateUnitCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public string UnitNumber { get; init; } = default!;
    public string? Floor { get; init; }
    public UnitType UnitType { get; init; }
    public decimal? Area { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool IsFurnished { get; init; }
    public bool HasParking { get; init; }
    public string? ElectricityMeterNumber { get; init; }
    public string? WaterMeterNumber { get; init; }
    public AcType? AcType { get; init; }
    public decimal? RentalPrice { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? DepositAmount { get; init; }
    public string? Description { get; init; }
}

public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UnitNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitType).IsInEnum();
        RuleFor(x => x.Area).GreaterThan(0).When(x => x.Area.HasValue);
        RuleFor(x => x.RentalPrice).GreaterThan(0).When(x => x.RentalPrice.HasValue);
        RuleFor(x => x.SalePrice).GreaterThan(0).When(x => x.SalePrice.HasValue);
    }
}

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateUnitCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Property), request.PropertyId);

        var duplicateExists = await _context.Units.AnyAsync(u =>
            u.PropertyId == request.PropertyId && u.UnitNumber == request.UnitNumber && !u.IsDeleted, cancellationToken);
        if (duplicateExists)
        {
            throw new Common.Exceptions.BusinessRuleException($"يوجد بالفعل وحدة بنفس الرقم ({request.UnitNumber}) في هذا العقار.");
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("UNIT", companyId, cancellationToken);

        var unit = new Unit
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            PropertyId = property.Id,
            UnitCode = code,
            UnitNumber = request.UnitNumber,
            Floor = request.Floor,
            UnitType = request.UnitType,
            CurrentStatus = UnitStatus.Available,
            Area = request.Area,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            IsFurnished = request.IsFurnished,
            HasParking = request.HasParking,
            ElectricityMeterNumber = request.ElectricityMeterNumber,
            WaterMeterNumber = request.WaterMeterNumber,
            AcType = request.AcType,
            RentalPrice = request.RentalPrice,
            SalePrice = request.SalePrice,
            DepositAmount = request.DepositAmount,
            Description = request.Description
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync(cancellationToken);

        return unit.Id;
    }
}
