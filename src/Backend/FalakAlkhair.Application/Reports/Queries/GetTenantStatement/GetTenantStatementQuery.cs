using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Reports.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetTenantStatement;

public record GetTenantStatementQuery(Guid TenantId) : IRequest<TenantStatementDto>;

public class GetTenantStatementQueryHandler : IRequestHandler<GetTenantStatementQuery, TenantStatementDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTenantStatementQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TenantStatementDto> Handle(GetTenantStatementQuery request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var tenant = await _context.Tenants
            .AsNoTracking()
            .Where(t => t.CompanyId == companyId && !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Tenant), request.TenantId);

        var leases = await _context.Leases
            .AsNoTracking()
            .Include(l => l.Property)
            .Include(l => l.Unit)
            .Include(l => l.Payments)
            .Where(l => l.TenantId == request.TenantId && !l.IsDeleted)
            .ToListAsync(cancellationToken);

        var leaseLines = leases.Select(l => new TenantStatementLeaseLineDto
        {
            LeaseId = l.Id,
            LeaseNumber = l.LeaseNumber,
            PropertyName = l.Property.PropertyName,
            UnitNumber = l.Unit.UnitNumber,
            Status = l.Status.ToString(),
            AnnualRentAmount = l.AnnualRentAmount,
            TotalPaid = l.Payments.Sum(p => p.PaidAmount),
            Outstanding = l.Payments.Sum(p => p.Amount - p.PaidAmount)
        }).ToList();

        var today = DateTime.UtcNow.Date;
        var overdueCount = leases.SelectMany(l => l.Payments)
            .Count(p => p.Status != LeasePaymentStatus.Paid && p.Status != LeasePaymentStatus.Cancelled && p.DueDate.Date < today);

        return new TenantStatementDto
        {
            TenantId = tenant.Id,
            TenantNameAr = tenant.NameAr,
            TotalRentDue = leaseLines.Sum(l => l.AnnualRentAmount),
            TotalPaid = leaseLines.Sum(l => l.TotalPaid),
            OutstandingBalance = leaseLines.Sum(l => l.Outstanding),
            OverdueInstallmentsCount = overdueCount,
            Leases = leaseLines
        };
    }
}
