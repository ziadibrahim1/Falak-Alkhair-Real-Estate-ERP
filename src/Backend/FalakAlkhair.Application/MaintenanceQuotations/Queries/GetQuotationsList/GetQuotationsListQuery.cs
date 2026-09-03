using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceQuotations.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceQuotations.Queries.GetQuotationsList;

public class GetQuotationsListQuery : ListQueryParams, IRequest<PaginatedList<MaintenanceQuotationDto>>
{
    public Guid? MaintenanceRequestId { get; init; }
    public QuotationStatus? Status { get; init; }
}

public class GetQuotationsListQueryHandler : IRequestHandler<GetQuotationsListQuery, PaginatedList<MaintenanceQuotationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuotationsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<MaintenanceQuotationDto>> Handle(GetQuotationsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaintenanceQuotations
            .AsNoTracking()
            .Include(q => q.Vendor)
            .Include(q => q.MaintenanceRequest)
            .Include(q => q.Items)
            .Where(q => q.CompanyId == _currentUser.CompanyId && !q.IsDeleted);

        if (request.MaintenanceRequestId.HasValue) query = query.Where(q => q.MaintenanceRequestId == request.MaintenanceRequestId.Value);
        if (request.Status.HasValue) query = query.Where(q => q.Status == request.Status.Value);

        query = request.SortDescending
            ? query.OrderByDescending(q => q.TotalAmount)
            : query.OrderBy(q => q.CreatedAt);

        var projected = query.Select(q => MaintenanceQuotationDto.FromEntity(q));

        return await PaginatedList<MaintenanceQuotationDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
