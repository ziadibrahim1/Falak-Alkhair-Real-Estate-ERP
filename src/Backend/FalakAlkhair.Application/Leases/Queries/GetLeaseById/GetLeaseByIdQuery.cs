using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Leases.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Queries.GetLeaseById;

public record GetLeaseByIdQuery(Guid Id) : IRequest<LeaseDto>;

public class GetLeaseByIdQueryHandler : IRequestHandler<GetLeaseByIdQuery, LeaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLeaseByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LeaseDto> Handle(GetLeaseByIdQuery request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .AsNoTracking()
            .Include(l => l.Tenant)
            .Include(l => l.Owner)
            .Include(l => l.Property)
            .Include(l => l.Unit)
            .Include(l => l.Agent)
            .Include(l => l.Payments)
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Lease), request.Id);

        return new LeaseDto
        {
            Id = lease.Id,
            LeaseNumber = lease.LeaseNumber,
            TenantId = lease.TenantId,
            TenantNameAr = lease.Tenant.NameAr,
            OwnerId = lease.OwnerId,
            OwnerNameAr = lease.Owner.NameAr,
            PropertyId = lease.PropertyId,
            PropertyName = lease.Property.PropertyName,
            UnitId = lease.UnitId,
            UnitNumber = lease.Unit.UnitNumber,
            AgentId = lease.AgentId,
            AgentNameAr = lease.Agent?.NameAr,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            AnnualRentAmount = lease.AnnualRentAmount,
            PaymentFrequency = lease.PaymentFrequency,
            NumberOfPayments = lease.NumberOfPayments,
            SecurityDeposit = lease.SecurityDeposit,
            CommissionPercentage = lease.CommissionPercentage,
            VatPercentage = lease.VatPercentage,
            Status = lease.Status,
            DaysRemaining = (lease.EndDate.Date - DateTime.UtcNow.Date).Days,
            Notes = lease.Notes,
            CreatedAt = lease.CreatedAt,
            Payments = lease.Payments
                .OrderBy(p => p.InstallmentNumber)
                .Select(LeasePaymentDto.FromEntity)
                .ToList()
        };
    }
}
