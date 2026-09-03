using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetCommissionSummaryReport;

public class CommissionSummaryLineDto
{
    public Guid AgentId { get; set; }
    public string AgentNameAr { get; set; } = default!;
    public int CommissionsCount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
}

/// <summary>ملخّص عمولات المسوّقين (كل المصادر: إيجار/بيع/مزاد) مجمَّعًا حسب المسوّق.</summary>
public record GetCommissionSummaryReportQuery : IRequest<List<CommissionSummaryLineDto>>;

public class GetCommissionSummaryReportQueryHandler : IRequestHandler<GetCommissionSummaryReportQuery, List<CommissionSummaryLineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCommissionSummaryReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CommissionSummaryLineDto>> Handle(GetCommissionSummaryReportQuery request, CancellationToken cancellationToken)
    {
        return await _context.Commissions
            .AsNoTracking()
            .Include(c => c.Agent)
            .Where(c => c.CompanyId == _currentUser.CompanyId && !c.IsDeleted)
            .GroupBy(c => new { c.AgentId, c.Agent.NameAr })
            .Select(g => new CommissionSummaryLineDto
            {
                AgentId = g.Key.AgentId,
                AgentNameAr = g.Key.NameAr,
                CommissionsCount = g.Count(),
                PendingAmount = g.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.NetCommissionAmount),
                ApprovedAmount = g.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.NetCommissionAmount),
                PaidAmount = g.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.NetCommissionAmount),
                TotalNetAmount = g.Sum(c => c.NetCommissionAmount)
            })
            .OrderByDescending(l => l.TotalNetAmount)
            .ToListAsync(cancellationToken);
    }
}
