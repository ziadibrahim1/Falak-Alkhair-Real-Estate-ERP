using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Units.Commands.CreateUnit;
using FalakAlkhair.Application.Units.Commands.DeleteUnit;
using FalakAlkhair.Application.Units.Commands.UpdateUnit;
using FalakAlkhair.Application.Units.Queries.GetUnitById;
using FalakAlkhair.Application.Units.Queries.GetUnitsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة الوحدات العقارية (Units).</summary>
public class UnitsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.UnitView)]
    public async Task<IActionResult> GetList([FromQuery] GetUnitsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.UnitView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetUnitByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.UnitCreate)]
    public async Task<IActionResult> Create([FromBody] CreateUnitCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء الوحدة بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.UnitEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم تحديث بيانات الوحدة بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.UnitDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteUnitCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم حذف الوحدة بنجاح."));
    }
}
