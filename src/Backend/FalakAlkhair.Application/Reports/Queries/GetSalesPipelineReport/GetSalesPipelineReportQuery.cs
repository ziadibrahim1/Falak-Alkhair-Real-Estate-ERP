using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetSalesPipelineReport;

public class SalesPipelineStageDto
{
    public string Stage { get; set; } = default!;
    public int Count { get; set; }
    public decimal TotalAskingValue { get; set; }
}

/// <summary>ملخّص مسار المبيعات (Sales Pipeline) مجمَّعًا حسب المرحلة.</summary>
public record GetSalesPipelineReportQuery : IRequest<List<SalesPipelineStageDto>>;

public class GetSalesPipelineReportQueryHandler : IRequestHandler<GetSalesPipelineReportQuery, List<SalesPipelineStageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSalesPipelineReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<SalesPipelineStageDto>> Handle(GetSalesPipelineReportQuery request, CancellationToken cancellationToken)
    {
        var grouped = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CompanyId == _currentUser.CompanyId && !s.IsDeleted)
            .GroupBy(s => s.Stage)
            .Select(g => new SalesPipelineStageDto
            {
                Stage = g.Key.ToString(),
                Count = g.Count(),
                TotalAskingValue = g.Sum(s => s.AskingPrice)
            })
            .ToListAsync(cancellationToken);

        var order = Enum.GetValues<SaleStage>().Select(s => s.ToString()).ToList();
        return grouped.OrderBy(g => order.IndexOf(g.Stage)).ToList();
    }
}
