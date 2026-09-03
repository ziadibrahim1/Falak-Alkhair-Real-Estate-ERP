using FalakAlkhair.Application.Buyers.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Buyers.Queries.GetBuyerMatches;

/// <summary>
/// محرك مطابقة بسيط (Buyer-Property Matching): يعيد الوحدات المعروضة للبيع
/// التي تطابق معايير المشتري (المدينة، النوع، المساحة، السعر ضمن الميزانية).
/// مطابقة قواعدية (Rule-based) حقيقية على بيانات فعلية — وليست AI، تحقيقًا
/// لمتطلب "لا تضف AI API حقيقيًا إلا إذا طُلب صراحة" (راجع البند 61 و16).
/// </summary>
public record GetBuyerMatchesQuery(Guid BuyerId) : IRequest<List<PropertyMatchDto>>;

public class GetBuyerMatchesQueryHandler : IRequestHandler<GetBuyerMatchesQuery, List<PropertyMatchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBuyerMatchesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PropertyMatchDto>> Handle(GetBuyerMatchesQuery request, CancellationToken cancellationToken)
    {
        var buyer = await _context.Buyers
            .AsNoTracking()
            .Where(b => b.CompanyId == _currentUser.CompanyId && !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.Id == request.BuyerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Buyer), request.BuyerId);

        var unitsQuery = _context.Units
            .AsNoTracking()
            .Include(u => u.Property)
            .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted
                && u.CurrentStatus == UnitStatus.ListedForSale
                && u.SalePrice != null);

        if (buyer.Budget.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.SalePrice!.Value <= buyer.Budget.Value);
        }

        if (!string.IsNullOrWhiteSpace(buyer.PreferredCity))
        {
            unitsQuery = unitsQuery.Where(u => u.Property.City == buyer.PreferredCity);
        }

        if (!string.IsNullOrWhiteSpace(buyer.PreferredDistrict))
        {
            unitsQuery = unitsQuery.Where(u => u.Property.District == buyer.PreferredDistrict);
        }

        if (buyer.PreferredPropertyType.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.Property.PropertyType == buyer.PreferredPropertyType.Value);
        }

        if (buyer.MinArea.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.Area == null || u.Area.Value >= buyer.MinArea.Value);
        }

        if (buyer.MaxArea.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.Area == null || u.Area.Value <= buyer.MaxArea.Value);
        }

        return await unitsQuery
            .OrderBy(u => u.SalePrice)
            .Take(50)
            .Select(u => new PropertyMatchDto
            {
                PropertyId = u.Property.Id,
                PropertyCode = u.Property.PropertyCode,
                PropertyName = u.Property.PropertyName,
                PropertyType = u.Property.PropertyType,
                City = u.Property.City,
                District = u.Property.District,
                TotalArea = u.Property.TotalArea,
                UnitId = u.Id,
                UnitCode = u.UnitCode,
                UnitNumber = u.UnitNumber,
                Area = u.Area,
                SalePrice = u.SalePrice
            })
            .ToListAsync(cancellationToken);
    }
}
