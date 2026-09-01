using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Properties.Commands.CreateProperty;

public record CreatePropertyCommand : IRequest<Guid>
{
    public string PropertyName { get; init; } = default!;
    public PropertyType PropertyType { get; init; }
    public PropertyCategory PropertyCategory { get; init; }
    public Guid OwnerId { get; init; }
    public string? DeedNumber { get; init; }
    public DateTime? DeedDate { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public string? Street { get; init; }
    public string? BuildingNumber { get; init; }
    public string? AdditionalNumber { get; init; }
    public string? PostalCode { get; init; }
    public string? NationalAddressShortCode { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? TotalArea { get; init; }
    public decimal? BuildingArea { get; init; }
    public int? NumberOfFloors { get; init; }
    public int? YearBuilt { get; init; }
    public string? Description { get; init; }
    public Guid? ManagerUserId { get; init; }
}

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyName).NotEmpty().WithMessage("اسم العقار مطلوب.").MaximumLength(300);
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("يجب تحديد مالك العقار.");
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.PropertyCategory).IsInEnum();
        RuleFor(x => x.TotalArea).GreaterThan(0).When(x => x.TotalArea.HasValue)
            .WithMessage("المساحة يجب أن تكون أكبر من صفر.");
        RuleFor(x => x.YearBuilt).InclusiveBetween(1300, 1500)
            .When(x => x.YearBuilt.HasValue && x.YearBuilt > 1300)
            .WithMessage("سنة البناء غير منطقية.");
    }
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreatePropertyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var ownerExists = await _context.Owners
            .AnyAsync(o => o.Id == request.OwnerId && o.CompanyId == companyId && !o.IsDeleted, cancellationToken);
        if (!ownerExists)
        {
            throw new NotFoundException(nameof(Owner), request.OwnerId);
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("PROPERTY", companyId, cancellationToken);

        var property = new Property
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            PropertyCode = code,
            PropertyName = request.PropertyName,
            PropertyType = request.PropertyType,
            PropertyCategory = request.PropertyCategory,
            Status = PropertyStatus.Active,
            OwnerId = request.OwnerId,
            DeedNumber = request.DeedNumber,
            DeedDate = request.DeedDate,
            City = request.City,
            District = request.District,
            Street = request.Street,
            BuildingNumber = request.BuildingNumber,
            AdditionalNumber = request.AdditionalNumber,
            PostalCode = request.PostalCode,
            NationalAddressShortCode = request.NationalAddressShortCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TotalArea = request.TotalArea,
            BuildingArea = request.BuildingArea,
            NumberOfFloors = request.NumberOfFloors,
            YearBuilt = request.YearBuilt,
            Description = request.Description,
            ManagerUserId = request.ManagerUserId
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync(cancellationToken);

        return property.Id;
    }
}
