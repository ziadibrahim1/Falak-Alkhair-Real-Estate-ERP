using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Tenants.Commands.CreateTenant;
using FalakAlkhair.Application.Tenants.Commands.DeleteTenant;
using FalakAlkhair.Application.Tenants.Commands.UpdateTenant;
using FalakAlkhair.Application.Tenants.Queries.GetTenantById;
using FalakAlkhair.Application.Tenants.Queries.GetTenantsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة المستأجرين (Tenant CRM).</summary>
public class TenantsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.TenantView)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.TenantView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetTenantByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.TenantCreate)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة المستأجر بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.TenantEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات المستأجر بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.TenantDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteTenantCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف المستأجر بنجاح."));
    }
}
