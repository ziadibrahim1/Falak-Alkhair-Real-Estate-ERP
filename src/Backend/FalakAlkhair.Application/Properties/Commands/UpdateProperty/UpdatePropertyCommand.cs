using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand : IRequest
{
    public Guid Id { get; init; }
    public string PropertyName { get; init; } = default!;
    public PropertyStatus Status { get; init; }
    public string? DeedNumber { get; init; }
    public DateTime? DeedDate { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public string? Street { get; init; }
    public string? BuildingNumber { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? TotalArea { get; init; }
    public decimal? BuildingArea { get; init; }
    public string? Description { get; init; }
}

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PropertyName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePropertyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Property), request.Id);

        property.PropertyName = request.PropertyName;
        property.Status = request.Status;
        property.DeedNumber = request.DeedNumber;
        property.DeedDate = request.DeedDate;
        property.City = request.City;
        property.District = request.District;
        property.Street = request.Street;
        property.BuildingNumber = request.BuildingNumber;
        property.Latitude = request.Latitude;
        property.Longitude = request.Longitude;
        property.TotalArea = request.TotalArea;
        property.BuildingArea = request.BuildingArea;
        property.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
