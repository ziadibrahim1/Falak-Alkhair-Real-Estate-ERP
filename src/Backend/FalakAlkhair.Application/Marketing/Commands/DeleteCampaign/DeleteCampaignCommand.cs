using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Marketing.Commands.DeleteCampaign;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteCampaignCommand(Guid Id) : IRequest;

public class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteCampaignCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.MarketingCampaigns
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MarketingCampaign), request.Id);

        campaign.IsDeleted = true;
        campaign.DeletedAt = DateTime.UtcNow;
        campaign.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
