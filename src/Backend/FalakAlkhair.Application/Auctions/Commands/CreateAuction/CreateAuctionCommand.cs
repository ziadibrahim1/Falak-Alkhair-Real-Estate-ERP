using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public Guid? UnitId { get; init; }
    public Guid? SellerId { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal StartingPrice { get; init; }
    public decimal? ReservePrice { get; init; }
    public decimal? DepositAmount { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal VatPercentage { get; init; } = 15;
    public string? Notes { get; init; }
}

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("تاريخ نهاية المزاد يجب أن يكون بعد تاريخ البداية.");
        RuleFor(x => x.StartingPrice).GreaterThan(0).WithMessage("سعر الافتتاح يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.ReservePrice).GreaterThanOrEqualTo(x => x.StartingPrice).When(x => x.ReservePrice.HasValue)
            .WithMessage("السعر الاحتياطي يجب ألا يقل عن سعر الافتتاح.");
        RuleFor(x => x.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.VatPercentage).InclusiveBetween(0, 100);
    }
}

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberGeneratorService _numberGenerator;

    public CreateAuctionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, INumberGeneratorService numberGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var property = await _context.Properties
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Property), request.PropertyId);

        if (request.UnitId.HasValue)
        {
            var unit = await _context.Units
                .Where(u => u.CompanyId == companyId && !u.IsDeleted)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId.Value);

            if (unit.PropertyId != request.PropertyId)
            {
                throw new Common.Exceptions.BusinessRuleException("الوحدة المحددة لا تنتمي للعقار المحدد.");
            }
        }

        var code = await _numberGenerator.GenerateNextNumberAsync("AUCT", companyId, cancellationToken);

        var auction = new Auction
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            AuctionNumber = code,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            OwnerId = property.OwnerId,
            SellerId = request.SellerId,
            AgentId = request.AgentId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StartingPrice = request.StartingPrice,
            ReservePrice = request.ReservePrice,
            DepositAmount = request.DepositAmount,
            CommissionPercentage = request.CommissionPercentage,
            VatPercentage = request.VatPercentage,
            Status = AuctionStatus.Draft,
            Notes = request.Notes
        };

        auction.AuditLogs.Add(new AuctionAuditLog
        {
            CompanyId = companyId,
            BranchId = _currentUser.BranchId,
            EventType = AuctionEventType.AuctionCreated,
            OccurredAt = DateTime.UtcNow,
            Notes = "تم إنشاء المزاد كمسودة."
        });

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync(cancellationToken);

        return auction.Id;
    }
}
