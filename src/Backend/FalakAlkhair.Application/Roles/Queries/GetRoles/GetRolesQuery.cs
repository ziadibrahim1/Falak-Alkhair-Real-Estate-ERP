using FalakAlkhair.Application.Common.Interfaces;
using MediatR;

namespace FalakAlkhair.Application.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IRoleManagementService _roleService;

    public GetRolesQueryHandler(IRoleManagementService roleService)
    {
        _roleService = roleService;
    }

    public Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        _roleService.GetRolesAsync(cancellationToken);
}
