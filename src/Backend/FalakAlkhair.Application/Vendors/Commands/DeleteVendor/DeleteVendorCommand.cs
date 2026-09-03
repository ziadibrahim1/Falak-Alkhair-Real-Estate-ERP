using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Vendors.Commands.DeleteVendor;

/// <summary>حذف ناعم (Soft Delete) فقط.</summary>
public record DeleteVendorCommand(Guid Id) : IRequest;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteVendorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.Vendors
            .Where(v => v.CompanyId == _currentUser.CompanyId && !v.IsDeleted)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vendor), request.Id);

        vendor.IsDeleted = true;
        vendor.DeletedAt = DateTime.UtcNow;
        vendor.DeletedBy = _currentUser.UserName;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
