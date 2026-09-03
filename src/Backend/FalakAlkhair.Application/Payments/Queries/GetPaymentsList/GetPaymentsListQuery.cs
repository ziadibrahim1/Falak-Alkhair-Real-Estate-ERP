using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Payments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Payments.Queries.GetPaymentsList;

public class GetPaymentsListQuery : ListQueryParams, IRequest<PaginatedList<PaymentDto>>
{
    public Guid? LeaseId { get; init; }
    public Guid? TenantId { get; init; }
}

public class GetPaymentsListQueryHandler : IRequestHandler<GetPaymentsListQuery, PaginatedList<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<PaymentDto>> Handle(GetPaymentsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p => p.PaymentNumber.Contains(term) || (p.ReferenceNumber != null && p.ReferenceNumber.Contains(term)));
        }

        if (request.LeaseId.HasValue) query = query.Where(p => p.LeaseId == request.LeaseId);
        if (request.TenantId.HasValue) query = query.Where(p => p.Lease.TenantId == request.TenantId);

        query = request.SortDescending
            ? query.OrderByDescending(p => p.PaymentDate)
            : query.OrderBy(p => p.PaymentDate);

        var projected = query.Select(p => new PaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            LeaseId = p.LeaseId,
            LeaseNumber = p.Lease.LeaseNumber,
            TenantId = p.Lease.TenantId,
            TenantNameAr = p.Lease.Tenant.NameAr,
            LeasePaymentId = p.LeasePaymentId,
            InstallmentNumber = p.LeasePayment != null ? p.LeasePayment.InstallmentNumber : null,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod,
            ReferenceNumber = p.ReferenceNumber,
            BankName = p.BankName,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt
        });

        return await PaginatedList<PaymentDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
