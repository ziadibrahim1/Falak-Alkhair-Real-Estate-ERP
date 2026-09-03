using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Viewings.Commands.CompleteViewing;

/// <summary>تسجيل نتيجة معاينة: Scheduled → Completed/Cancelled/NoShow.</summary>
public record CompleteViewingCommand : IRequest
{
    public Guid Id { get; init; }
    public ViewingStatus Status { get; init; }
    public string? Feedback { get; init; }
}

public class CompleteViewingCommandHandler : IRequestHandler<CompleteViewingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompleteViewingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CompleteViewingCommand request, CancellationToken cancellationToken)
    {
        var viewing = await _context.Viewings
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Viewing), request.Id);

        if (viewing.Status != ViewingStatus.Scheduled)
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن تحديث نتيجة معاينة غير مجدولة (Scheduled) حاليًا.");
        }

        if (request.Status == ViewingStatus.Scheduled)
        {
            throw new Common.Exceptions.BusinessRuleException("الحالة الجديدة يجب أن تكون Completed أو Cancelled أو NoShow.");
        }

        viewing.Status = request.Status;
        viewing.Feedback = request.Feedback;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
