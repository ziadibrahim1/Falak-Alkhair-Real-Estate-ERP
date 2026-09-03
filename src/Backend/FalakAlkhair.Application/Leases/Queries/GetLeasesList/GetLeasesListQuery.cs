using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Leases.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Leases.Queries.GetLeasesList;

public class GetLeasesListQuery : ListQueryParams, IRequest<PaginatedList<LeaseDto>>
{
    public LeaseStatus? Status { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? PropertyId { get; init; }
    /// <summary>عقود تنتهي خلال هذا العدد من الأيام (لتنبيهات الانتهاء القريب).</summary>
    public int? ExpiringWithinDays { get; init; }
}

public class GetLeasesListQueryHandler : IRequestHandler<GetLeasesListQuery, PaginatedList<LeaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLeasesListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<LeaseDto>> Handle(GetLeasesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leases
            .AsNoTracking()
            .Where(l => l.CompanyId == _currentUser.CompanyId && !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(l => l.LeaseNumber.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(l => l.Status == request.Status);
        if (request.TenantId.HasValue) query = query.Where(l => l.TenantId == request.TenantId);
        if (request.PropertyId.HasValue) query = query.Where(l => l.PropertyId == request.PropertyId);

        if (request.ExpiringWithinDays.HasValue)
        {
            var threshold = DateTime.UtcNow.Date.AddDays(request.ExpiringWithinDays.Value);
            query = query.Where(l => l.Status == LeaseStatus.Active && l.EndDate <= threshold);
        }

        query = request.SortDescending
            ? query.OrderByDescending(l => l.CreatedAt)
            : query.OrderBy(l => l.CreatedAt);

        var projected = query.Select(l => new LeaseDto
        {
            Id = l.Id,
            LeaseNumber = l.LeaseNumber,
            TenantId = l.TenantId,
            TenantNameAr = l.Tenant.NameAr,
            OwnerId = l.OwnerId,
            OwnerNameAr = l.Owner.NameAr,
            PropertyId = l.PropertyId,
            PropertyName = l.Property.PropertyName,
            UnitId = l.UnitId,
            UnitNumber = l.Unit.UnitNumber,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            AnnualRentAmount = l.AnnualRentAmount,
            PaymentFrequency = l.PaymentFrequency,
            NumberOfPayments = l.NumberOfPayments,
            SecurityDeposit = l.SecurityDeposit,
            CommissionPercentage = l.CommissionPercentage,
            VatPercentage = l.VatPercentage,
            Status = l.Status,
            DaysRemaining = (l.EndDate.Date - DateTime.UtcNow.Date).Days,
            CreatedAt = l.CreatedAt
        });

        return await PaginatedList<LeaseDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
