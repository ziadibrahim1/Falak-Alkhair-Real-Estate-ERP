using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Tenants.Commands.DeleteTenant;

/// <summary>حذف ناعم (Soft Delete) فقط — لا يُحذف أي سجل تعاقدي/مالي فعليًا.</summary>
public record DeleteTenantCommand(Guid Id) : IRequest;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTenantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .Where(t => t.CompanyId == _currentUser.CompanyId && !t.IsDeleted)
            .Include(t => t.Leases)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Tenant), request.Id);

        if (tenant.Leases.Any(l => l.Status == Domain.Common.Enums.LeaseStatus.Active))
        {
            throw new Common.Exceptions.BusinessRuleException("لا يمكن حذف مستأجر لديه عقود إيجار نشطة. أنهِ العقود أولًا.");
        }

        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;
        tenant.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
