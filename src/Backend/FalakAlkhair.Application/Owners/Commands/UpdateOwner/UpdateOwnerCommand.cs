using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? NationalAddress { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public string? BankName { get; init; }
    public string? Iban { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateOwnerCommandValidator : AbstractValidator<UpdateOwnerCommand>
{
    public UpdateOwnerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Iban).Matches(@"^SA[0-9]{22}$").When(x => !string.IsNullOrWhiteSpace(x.Iban));
    }
}

public class UpdateOwnerCommandHandler : IRequestHandler<UpdateOwnerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateOwnerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _context.Owners
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Owner), request.Id);

        owner.NameAr = request.NameAr;
        owner.NameEn = request.NameEn;
        owner.Mobile = request.Mobile;
        owner.Email = request.Email;
        owner.NationalAddress = request.NationalAddress;
        owner.City = request.City;
        owner.District = request.District;
        owner.BankName = request.BankName;
        owner.Iban = request.Iban;
        owner.Notes = request.Notes;
        owner.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
