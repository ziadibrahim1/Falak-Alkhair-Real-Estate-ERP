using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Roles.Commands.CreateRole;
using FalakAlkhair.Application.Roles.Queries.GetRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة الأدوار وصلاحياتها الديناميكية.</summary>
public class RolesController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.RoleView)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetRolesQuery(), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.RoleManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<Guid>.Ok(id, "تم إنشاء الدور بنجاح."));
    }

    /// <summary>قائمة كل الصلاحيات المتاحة في النظام لعرضها عند بناء دور جديد.</summary>
    [HttpGet("permissions")]
    [Authorize(Policy = "Permission:" + Permissions.RoleView)]
    public IActionResult GetAllPermissions()
        => Ok(ApiResponse<object>.Ok(Permissions.All.Select(p => new { p.Code, p.Module, p.Action, p.DescriptionAr })));
}
