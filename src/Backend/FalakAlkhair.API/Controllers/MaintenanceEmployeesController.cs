using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceEmployees.Commands.CreateMaintenanceEmployee;
using FalakAlkhair.Application.MaintenanceEmployees.Commands.DeleteMaintenanceEmployee;
using FalakAlkhair.Application.MaintenanceEmployees.Commands.UpdateMaintenanceEmployee;
using FalakAlkhair.Application.MaintenanceEmployees.Queries.GetMaintenanceEmployeeById;
using FalakAlkhair.Application.MaintenanceEmployees.Queries.GetMaintenanceEmployeesList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة فنيي الصيانة الداخليين القابلين لإسناد طلبات الصيانة إليهم.</summary>
public class MaintenanceEmployeesController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceEmployeeView)]
    public async Task<IActionResult> GetList([FromQuery] GetMaintenanceEmployeesListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceEmployeeView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetMaintenanceEmployeeByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceEmployeeCreate)]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceEmployeeCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة فني الصيانة بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceEmployeeEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaintenanceEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات الفني بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceEmployeeDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteMaintenanceEmployeeCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف الفني بنجاح."));
    }
}
