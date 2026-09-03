using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Offers.Commands.UpdateOfferStatus;

/// <summary>تحديث حالة عرض شراء: Pending → Accepted/Rejected/Withdrawn (أو Expired تلقائيًا عند انتهاء المدة).</summary>
public record UpdateOfferStatusCommand : IRequest
{
    public Guid Id { get; init; }
    public OfferStatus Status { get; init; }
}

public class UpdateOfferStatusCommandValidator : AbstractValidator<UpdateOfferStatusCommand>
{
    public UpdateOfferStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).NotEqual(OfferStatus.Pending).WithMessage("لا يمكن إعادة تعيين حالة عرض إلى Pending.");
    }
}

public class UpdateOfferStatusCommandHandler : IRequestHandler<UpdateOfferStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateOfferStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateOfferStatusCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.Offers
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Offer), request.Id);

        if (offer.Status != OfferStatus.Pending)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن تحديث حالة عرض تم البت فيه بالفعل.");
        }

        offer.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
