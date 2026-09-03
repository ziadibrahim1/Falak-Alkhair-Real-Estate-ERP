using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public string? NationalAddress { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public string? Employer { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateTenantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .Where(t => t.CompanyId == _currentUser.CompanyId && !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Tenant), request.Id);

        tenant.NameAr = request.NameAr;
        tenant.NameEn = request.NameEn;
        tenant.Mobile = request.Mobile;
        tenant.Email = request.Email;
        tenant.NationalAddress = request.NationalAddress;
        tenant.City = request.City;
        tenant.District = request.District;
        tenant.Employer = request.Employer;
        tenant.Notes = request.Notes;
        tenant.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
