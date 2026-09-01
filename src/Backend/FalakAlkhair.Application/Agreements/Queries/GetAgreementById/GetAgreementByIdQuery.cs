using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Agreements.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Agreements.Queries.GetAgreementById;

public record GetAgreementByIdQuery(Guid Id) : IRequest<AgreementDto>;

public class GetAgreementByIdQueryHandler : IRequestHandler<GetAgreementByIdQuery, AgreementDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAgreementByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AgreementDto> Handle(GetAgreementByIdQuery request, CancellationToken cancellationToken)
    {
        var agreement = await _context.PropertyManagementAgreements
            .AsNoTracking()
            .Include(a => a.Owner)
            .Include(a => a.Property)
            .Where(a => a.CompanyId == _currentUser.CompanyId && !a.IsDeleted)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.PropertyManagementAgreement), request.Id);

        return AgreementDto.FromEntity(agreement);
    }
}
