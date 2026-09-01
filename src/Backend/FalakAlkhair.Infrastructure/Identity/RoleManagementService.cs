using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Infrastructure.Identity;

public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RoleManagementService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _roleManager = roleManager;
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .Where(r => r.CompanyId == null || r.CompanyId == _currentUser.CompanyId)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();
        var permissionsByRole = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => new { rp.RoleId, rp.Permission.Code })
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
            r.Id, r.Name!, r.NameAr, r.Description, r.IsSystemRole,
            permissionsByRole.Where(p => p.RoleId == r.Id).Select(p => p.Code).ToList())).ToList();
    }

    public async Task<Guid> CreateRoleAsync(string name, string nameAr, string? description, IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(name))
        {
            throw new BusinessRuleException($"يوجد بالفعل دور بالاسم \"{name}\".");
        }

        var role = new ApplicationRole
        {
            Name = name,
            NameAr = nameAr,
            Description = description,
            IsSystemRole = false,
            CompanyId = _currentUser.CompanyId
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await AssignPermissionsAsync(role.Id, permissionCodes, cancellationToken);

        return role.Id;
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationRole), roleId);

        if (role.IsSystemRole)
        {
            throw new BusinessRuleException("لا يمكن تعديل صلاحيات دور نظام أساسي (System Role) محمي.");
        }

        var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
        _context.RolePermissions.RemoveRange(existing);
        await _context.SaveChangesAsync(cancellationToken);

        await AssignPermissionsAsync(roleId, permissionCodes, cancellationToken);
    }

    private async Task AssignPermissionsAsync(Guid roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        var codes = permissionCodes.Distinct().ToList();
        var permissions = await _context.Permissions.Where(p => codes.Contains(p.Code)).ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permission.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
