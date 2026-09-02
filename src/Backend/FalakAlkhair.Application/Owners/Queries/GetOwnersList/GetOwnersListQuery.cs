using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Owners.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Owners.Queries.GetOwnersList;

   public class GetOwnersListQuery : ListQueryParams, IRequest<PaginatedList<OwnerDto>>
{
    public bool? IsActive { get; init; }
}

public class GetOwnersListQueryHandler : IRequestHandler<GetOwnersListQuery, PaginatedList<OwnerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOwnersListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OwnerDto>> Handle(GetOwnersListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Owners
            .AsNoTracking()
            .Where(o => o.CompanyId == _currentUser.CompanyId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o =>
                o.NameAr.Contains(term) ||
                (o.NameEn != null && o.NameEn.Contains(term)) ||
                o.OwnerCode.Contains(term) ||
                o.Mobile.Contains(term) ||
                (o.NationalId != null && o.NationalId.Contains(term)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(o => o.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(o => o.NameAr) : query.OrderBy(o => o.NameAr),
            "code" => request.SortDescending ? query.OrderByDescending(o => o.OwnerCode) : query.OrderBy(o => o.OwnerCode),
            _ => request.SortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };

        var projected = query.Select(o => new OwnerDto
        {
            Id = o.Id,
            OwnerCode = o.OwnerCode,
            PartyType = o.PartyType,
            NameAr = o.NameAr,
            NameEn = o.NameEn,
            NationalId = o.NationalId,
            CommercialRegistrationNumber = o.CommercialRegistrationNumber,
            Mobile = o.Mobile,
            Email = o.Email,
            NationalAddress = o.NationalAddress,
            City = o.City,
            District = o.District,
            BankName = o.BankName,
            Iban = o.Iban,
            Notes = o.Notes,
            IsActive = o.IsActive,
            PropertiesCount = o.Properties.Count(p => !p.IsDeleted),
            CreatedAt = o.CreatedAt
        });

        return await PaginatedList<OwnerDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
