using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Payments.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Payments.Queries.GetOverduePayments;

/// <summary>لوحة المتأخرات (البند 15): كل قسط تجاوز تاريخ استحقاقه ولم يُسدَّد بالكامل.</summary>
public class GetOverduePaymentsQuery : ListQueryParams, IRequest<PaginatedList<OverduePaymentDto>>
{
}

public class GetOverduePaymentsQueryHandler : IRequestHandler<GetOverduePaymentsQuery, PaginatedList<OverduePaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOverduePaymentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OverduePaymentDto>> Handle(GetOverduePaymentsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var query = _context.LeasePayments
            .AsNoTracking()
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted &&
                        p.Status != LeasePaymentStatus.Cancelled &&
                        p.PaidAmount < p.Amount &&
                        p.DueDate.Date < today);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p => p.Lease.LeaseNumber.Contains(term) || p.Lease.Tenant.NameAr.Contains(term));
        }

        query = query.OrderBy(p => p.DueDate);

        var projected = query.Select(p => new OverduePaymentDto
        {
            LeasePaymentId = p.Id,
            LeaseId = p.LeaseId,
            LeaseNumber = p.Lease.LeaseNumber,
            TenantId = p.Lease.TenantId,
            TenantNameAr = p.Lease.Tenant.NameAr,
            TenantMobile = p.Lease.Tenant.Mobile,
            InstallmentNumber = p.InstallmentNumber,
            DueDate = p.DueDate,
            Amount = p.Amount,
            PaidAmount = p.PaidAmount,
            RemainingAmount = p.Amount - p.PaidAmount,
            DaysOverdue = 0 // يُحتسب فعليًا أدناه بعد التنفيذ لأن EF Core لا يترجم DateTime.UtcNow داخل Select بشكل مضمون عبر كل السيناريوهات
        });

        var result = await PaginatedList<OverduePaymentDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
        foreach (var item in result.Items)
        {
            item.DaysOverdue = (today - item.DueDate.Date).Days;
        }

        return result;
    }
}
