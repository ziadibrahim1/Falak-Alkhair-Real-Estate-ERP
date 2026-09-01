using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Units.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Units.Queries.GetUnitsList;

public record GetUnitsListQuery : ListQueryParams, IRequest<PaginatedList<UnitDto>>
{
    public Guid? PropertyId { get; init; }
    public UnitStatus? Status { get; init; }
    public UnitType? UnitType { get; init; }
}

public class GetUnitsListQueryHandler : IRequestHandler<GetUnitsListQuery, PaginatedList<UnitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnitsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<UnitDto>> Handle(GetUnitsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Units
            .AsNoTracking()
            .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(u => u.UnitNumber.Contains(term) || u.UnitCode.Contains(term));
        }

        if (request.PropertyId.HasValue) query = query.Where(u => u.PropertyId == request.PropertyId);
        if (request.Status.HasValue) query = query.Where(u => u.CurrentStatus == request.Status);
        if (request.UnitType.HasValue) query = query.Where(u => u.UnitType == request.UnitType);

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(u => u.UnitCode) : query.OrderBy(u => u.UnitCode),
            _ => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var projected = query.Select(u => new UnitDto
        {
            Id = u.Id,
            UnitCode = u.UnitCode,
            UnitNumber = u.UnitNumber,
            PropertyId = u.PropertyId,
            PropertyName = u.Property.PropertyName,
            Floor = u.Floor,
            UnitType = u.UnitType,
            CurrentStatus = u.CurrentStatus,
            Area = u.Area,
            Bedrooms = u.Bedrooms,
            Bathrooms = u.Bathrooms,
            IsFurnished = u.IsFurnished,
            HasParking = u.HasParking,
            RentalPrice = u.RentalPrice,
            SalePrice = u.SalePrice,
            DepositAmount = u.DepositAmount,
            Description = u.Description,
            CreatedAt = u.CreatedAt
        });

        return await PaginatedList<UnitDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
