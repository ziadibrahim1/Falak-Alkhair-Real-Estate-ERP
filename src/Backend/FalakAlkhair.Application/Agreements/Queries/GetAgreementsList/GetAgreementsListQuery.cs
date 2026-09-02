using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Agreements.DTOs;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agreements.Queries.GetAgreementsList;

public class GetAgreementsListQuery : ListQueryParams, IRequest<PaginatedList<AgreementDto>>
{
    public ManagementAgreementStatus? Status { get; init; }
    /// <summary>عقود تنتهي خلال هذا العدد من الأيام (لتنبيهات الانتهاء القريب).</summary>
    public int? ExpiringWithinDays { get; init; }
}

public class GetAgreementsListQueryHandler : IRequestHandler<GetAgreementsListQuery, PaginatedList<AgreementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAgreementsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<AgreementDto>> Handle(GetAgreementsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PropertyManagementAgreements
            .AsNoTracking()
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a => a.ContractNumber.Contains(term));
        }

        if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status);

        if (request.ExpiringWithinDays.HasValue)
        {
            var threshold = DateTime.UtcNow.Date.AddDays(request.ExpiringWithinDays.Value);
            query = query.Where(a => a.Status == ManagementAgreementStatus.Active && a.EndDate <= threshold);
        }

        query = request.SortDescending
            ? query.OrderByDescending(a => a.CreatedAt)
            : query.OrderBy(a => a.CreatedAt);

        var projected = query.Select(a => new AgreementDto
        {
            Id = a.Id,
            ContractNumber = a.ContractNumber,
            OwnerId = a.OwnerId,
            OwnerNameAr = a.Owner.NameAr,
            PropertyId = a.PropertyId,
            PropertyName = a.Property.PropertyName,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            ManagementFee = a.ManagementFee,
            CommissionType = a.CommissionType,
            CommissionPercentage = a.CommissionPercentage,
            Status = a.Status,
            DaysRemaining = (a.EndDate - DateTime.UtcNow).Days,
            CreatedAt = a.CreatedAt
        });

        return await PaginatedList<AgreementDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
