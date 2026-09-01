using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Units.Commands.UpdateUnit;

public record UpdateUnitCommand : IRequest
{
    public Guid Id { get; init; }
    public UnitStatus CurrentStatus { get; init; }
    public decimal? Area { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool IsFurnished { get; init; }
    public bool HasParking { get; init; }
    public decimal? RentalPrice { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? DepositAmount { get; init; }
    public string? Description { get; init; }
}

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CurrentStatus).IsInEnum();
    }
}

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateUnitCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units
            .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        unit.CurrentStatus = request.CurrentStatus;
        unit.Area = request.Area;
        unit.Bedrooms = request.Bedrooms;
        unit.Bathrooms = request.Bathrooms;
        unit.IsFurnished = request.IsFurnished;
        unit.HasParking = request.HasParking;
        unit.RentalPrice = request.RentalPrice;
        unit.SalePrice = request.SalePrice;
        unit.DepositAmount = request.DepositAmount;
        unit.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
