using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.MaintenanceQuotations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.MaintenanceQuotations.Queries.GetQuotationById;

public record GetQuotationByIdQuery(Guid Id) : IRequest<MaintenanceQuotationDto>;

public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, MaintenanceQuotationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuotationByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MaintenanceQuotationDto> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _context.MaintenanceQuotations
            .AsNoTracking()
            .Include(q => q.Vendor)
            .Include(q => q.MaintenanceRequest)
            .Include(q => q.Items)
            .Where(q => q.CompanyId == _currentUser.CompanyId && !q.IsDeleted)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.MaintenanceQuotation), request.Id);

        return MaintenanceQuotationDto.FromEntity(quotation);
    }
}
