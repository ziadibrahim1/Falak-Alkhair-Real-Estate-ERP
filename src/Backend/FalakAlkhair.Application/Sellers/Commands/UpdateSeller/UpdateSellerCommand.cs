using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Sellers.Commands.UpdateSeller;

public record UpdateSellerCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid? PropertyId { get; init; }
    public decimal AskingPrice { get; init; }
    public decimal? MinimumPrice { get; init; }
    public decimal CommissionPercentage { get; init; }
    public ListingMandateStatus MandateStatus { get; init; }
    public DateTime MandateStartDate { get; init; }
    public DateTime? MandateEndDate { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public string? Notes { get; init; }
}

public class UpdateSellerCommandValidator : AbstractValidator<UpdateSellerCommand>
{
    public UpdateSellerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AskingPrice).GreaterThan(0);
        RuleFor(x => x.MinimumPrice).LessThanOrEqualTo(x => x.AskingPrice).When(x => x.MinimumPrice.HasValue);
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MandateEndDate).GreaterThan(x => x.MandateStartDate).When(x => x.MandateEndDate.HasValue);
    }
}

public class UpdateSellerCommandHandler : IRequestHandler<UpdateSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSellerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.Sellers
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Seller), request.Id);

        seller.PropertyId = request.PropertyId;
        seller.AskingPrice = request.AskingPrice;
        seller.MinimumPrice = request.MinimumPrice;
        seller.CommissionPercentage = request.CommissionPercentage;
        seller.MandateStatus = request.MandateStatus;
        seller.MandateStartDate = request.MandateStartDate;
        seller.MandateEndDate = request.MandateEndDate;
        seller.AssignedAgentId = request.AssignedAgentId;
        seller.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
