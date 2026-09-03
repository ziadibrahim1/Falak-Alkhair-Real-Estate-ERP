using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Reports.Queries.GetOccupancyReport;

public class OccupancyLineDto
{
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public int TotalUnits { get; set; }
    public int RentedUnits { get; set; }
    public int SoldUnits { get; set; }
    public int AvailableUnits { get; set; }
    public decimal OccupancyRate { get; set; }
}

/// <summary>تقرير الإشغال (Occupancy) لكل عقار: نسبة الوحدات المؤجَّرة/المباعة من إجمالي وحداته.</summary>
public record GetOccupancyReportQuery : IRequest<List<OccupancyLineDto>>;

public class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, List<OccupancyLineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOccupancyReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<OccupancyLineDto>> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        var properties = await _context.Properties
            .AsNoTracking()
            .Include(p => p.Units)
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return properties.Select(p =>
        {
            var units = p.Units.Where(u => !u.IsDeleted).ToList();
            var rented = units.Count(u => u.CurrentStatus == UnitStatus.Rented);
            var sold = units.Count(u => u.CurrentStatus == UnitStatus.Sold);
            var available = units.Count(u => u.CurrentStatus == UnitStatus.Available);

            return new OccupancyLineDto
            {
                PropertyId = p.Id,
                PropertyName = p.PropertyName,
                TotalUnits = units.Count,
                RentedUnits = rented,
                SoldUnits = sold,
                AvailableUnits = available,
                OccupancyRate = units.Count == 0 ? 0 : Math.Round((decimal)(rented + sold) / units.Count * 100, 1)
            };
        }).ToList();
    }
}
