using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetMaintenanceSummaryReport;

public class MaintenanceSummaryLineDto
{
    public string Status { get; set; } = default!;
    public int Count { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
}

/// <summary>ملخّص طلبات الصيانة مجمَّعًا حسب الحالة.</summary>
public record GetMaintenanceSummaryReportQuery : IRequest<List<MaintenanceSummaryLineDto>>;

public class GetMaintenanceSummaryReportQueryHandler : IRequestHandler<GetMaintenanceSummaryReportQuery, List<MaintenanceSummaryLineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMaintenanceSummaryReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MaintenanceSummaryLineDto>> Handle(GetMaintenanceSummaryReportQuery request, CancellationToken cancellationToken)
    {
        return await _context.MaintenanceRequests
            .AsNoTracking()
            .Where(m => m.CompanyId == _currentUser.CompanyId && !m.IsDeleted)
            .GroupBy(m => m.Status)
            .Select(g => new MaintenanceSummaryLineDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                TotalEstimatedCost = g.Sum(m => m.EstimatedCost ?? 0),
                TotalActualCost = g.Sum(m => m.ActualCost ?? 0)
            })
            .ToListAsync(cancellationToken);
    }
}
