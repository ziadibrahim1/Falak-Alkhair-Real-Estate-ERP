using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Buyers.Commands.UpdateBuyer;

public record UpdateBuyerCommand : IRequest
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = default!;
    public string? NameEn { get; init; }
    public string Mobile { get; init; } = default!;
    public string? Email { get; init; }
    public decimal? Budget { get; init; }
    public string? PreferredCity { get; init; }
    public string? PreferredDistrict { get; init; }
    public PropertyType? PreferredPropertyType { get; init; }
    public decimal? MinArea { get; init; }
    public decimal? MaxArea { get; init; }
    public BuyerPurpose Purpose { get; init; }
    public FinancingStatus FinancingStatus { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateBuyerCommandValidator : AbstractValidator<UpdateBuyerCommand>
{
    public UpdateBuyerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mobile).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.MaxArea).GreaterThanOrEqualTo(x => x.MinArea)
            .When(x => x.MinArea.HasValue && x.MaxArea.HasValue);
    }
}

public class UpdateBuyerCommandHandler : IRequestHandler<UpdateBuyerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateBuyerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await _context.Buyers
            .Where(b => b.CompanyId == _currentUser.CompanyId && !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Buyer), request.Id);

        buyer.NameAr = request.NameAr;
        buyer.NameEn = request.NameEn;
        buyer.Mobile = request.Mobile;
        buyer.Email = request.Email;
        buyer.Budget = request.Budget;
        buyer.PreferredCity = request.PreferredCity;
        buyer.PreferredDistrict = request.PreferredDistrict;
        buyer.PreferredPropertyType = request.PreferredPropertyType;
        buyer.MinArea = request.MinArea;
        buyer.MaxArea = request.MaxArea;
        buyer.Purpose = request.Purpose;
        buyer.FinancingStatus = request.FinancingStatus;
        buyer.AssignedAgentId = request.AssignedAgentId;
        buyer.Notes = request.Notes;
        buyer.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
