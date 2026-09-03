using FalakAlkhair.Application.Agents.DTOs;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agents.Queries.GetAgentsList;

public class GetAgentsListQuery : ListQueryParams, IRequest<PaginatedList<AgentDto>>
{
    public AgentStatus? Status { get; init; }
    public bool? IsActive { get; init; }
}

public class GetAgentsListQueryHandler : IRequestHandler<GetAgentsListQuery, PaginatedList<AgentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAgentsListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<AgentDto>> Handle(GetAgentsListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Agents
            .AsNoTracking()
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a =>
                a.NameAr.Contains(term) ||
                (a.NameEn != null && a.NameEn.Contains(term)) ||
                a.AgentCode.Contains(term) ||
                a.Mobile.Contains(term) ||
                (a.FalLicenseNumber != null && a.FalLicenseNumber.Contains(term)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(a => a.NameAr) : query.OrderBy(a => a.NameAr),
            "code" => request.SortDescending ? query.OrderByDescending(a => a.AgentCode) : query.OrderBy(a => a.AgentCode),
            _ => request.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };

        var projected = query.Select(a => new AgentDto
        {
            Id = a.Id,
            AgentCode = a.AgentCode,
            NameAr = a.NameAr,
            NameEn = a.NameEn,
            NationalId = a.NationalId,
            Mobile = a.Mobile,
            Email = a.Email,
            FalLicenseNumber = a.FalLicenseNumber,
            FalLicenseExpiryDate = a.FalLicenseExpiryDate,
            Specialization = a.Specialization,
            ManagerUserId = a.ManagerUserId,
            Status = a.Status,
            CommissionSchemeType = a.CommissionSchemeType,
            DefaultCommissionPercentage = a.DefaultCommissionPercentage,
            DefaultCommissionFixedAmount = a.DefaultCommissionFixedAmount,
            Notes = a.Notes,
            IsActive = a.IsActive,
            CommissionsCount = a.Commissions.Count(c => !c.IsDeleted),
            CreatedAt = a.CreatedAt
        });

        return await PaginatedList<AgentDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
