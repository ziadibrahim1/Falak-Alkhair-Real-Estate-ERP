using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Properties.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Properties.Queries.GetPropertiesList;

public record GetPropertiesListQuery : ListQueryParams, IRequest<PaginatedList<PropertyDto>>
{
    public PropertyType? PropertyType { get; init; }
    public PropertyStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
    public string? City { get; init; }
}

public class GetPropertiesListQueryHandler : IRequestHandler<GetPropertiesListQuery, PaginatedList<PropertyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPropertiesListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<PropertyDto>> Handle(GetPropertiesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Properties
            .AsNoTracking()
            .Where(p => p.CompanyId == _currentUser.CompanyId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p =>
                p.PropertyName.Contains(term) ||
                p.PropertyCode.Contains(term) ||
                (p.DeedNumber != null && p.DeedNumber.Contains(term)));
        }

        if (request.PropertyType.HasValue) query = query.Where(p => p.PropertyType == request.PropertyType);
        if (request.Status.HasValue) query = query.Where(p => p.Status == request.Status);
        if (request.OwnerId.HasValue) query = query.Where(p => p.OwnerId == request.OwnerId);
        if (!string.IsNullOrWhiteSpace(request.City)) query = query.Where(p => p.City == request.City);

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.PropertyName) : query.OrderBy(p => p.PropertyName),
            "code" => request.SortDescending ? query.OrderByDescending(p => p.PropertyCode) : query.OrderBy(p => p.PropertyCode),
            _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        // ملاحظة: يُبنى التوقيع هنا كـ Select صريح (وليس استدعاء FromEntity) لأن
        // EF Core لا يستطيع ترجمة استدعاء دالة C# عشوائية داخل LINQ-to-Entities.
        var projected = query.Select(p => new PropertyDto
        {
            Id = p.Id,
            PropertyCode = p.PropertyCode,
            PropertyName = p.PropertyName,
            PropertyType = p.PropertyType,
            PropertyCategory = p.PropertyCategory,
            Status = p.Status,
            OwnerId = p.OwnerId,
            OwnerNameAr = p.Owner.NameAr,
            DeedNumber = p.DeedNumber,
            DeedDate = p.DeedDate,
            City = p.City,
            District = p.District,
            Street = p.Street,
            BuildingNumber = p.BuildingNumber,
            NationalAddressShortCode = p.NationalAddressShortCode,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            TotalArea = p.TotalArea,
            BuildingArea = p.BuildingArea,
            NumberOfFloors = p.NumberOfFloors,
            YearBuilt = p.YearBuilt,
            Description = p.Description,
            UnitsCount = p.Units.Count(u => !u.IsDeleted),
            AvailableUnitsCount = p.Units.Count(u => !u.IsDeleted && u.CurrentStatus == UnitStatus.Available),
            CreatedAt = p.CreatedAt
        });

        return await PaginatedList<PropertyDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
