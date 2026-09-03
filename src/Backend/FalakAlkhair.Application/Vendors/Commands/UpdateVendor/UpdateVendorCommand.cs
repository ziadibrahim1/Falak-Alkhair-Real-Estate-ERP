using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Vendors.Commands.UpdateVendor;

public record UpdateVendorCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string? ContactPerson { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? CommercialRegistrationNumber { get; init; }
    public string? VatNumber { get; init; }
    public string? Services { get; init; }
    public decimal? Rating { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Rating).InclusiveBetween(0, 5).When(x => x.Rating.HasValue);
    }
}

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateVendorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vendor), request.Id);

        vendor.NameAr = request.NameAr;
        vendor.ContactPerson = request.ContactPerson;
        vendor.Mobile = request.Mobile;
        vendor.Email = request.Email;
        vendor.CommercialRegistrationNumber = request.CommercialRegistrationNumber;
        vendor.VatNumber = request.VatNumber;
        vendor.Services = request.Services;
        vendor.Rating = request.Rating;
        vendor.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
