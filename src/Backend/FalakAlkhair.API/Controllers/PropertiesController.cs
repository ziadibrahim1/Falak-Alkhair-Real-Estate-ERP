using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Properties.Commands.CreateProperty;
using FalakAlkhair.Application.Properties.Commands.DeleteProperty;
using FalakAlkhair.Application.Properties.Commands.UpdateProperty;
using FalakAlkhair.Application.Properties.Queries.GetPropertiesList;
using FalakAlkhair.Application.Properties.Queries.GetPropertyById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة العقارات (Properties).</summary>
public class PropertiesController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.PropertyView)]
    public async Task<IActionResult> GetList([FromQuery] GetPropertiesListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.PropertyView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetPropertyByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.PropertyCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePropertyCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء العقار بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.PropertyEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات العقار بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.PropertyDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeletePropertyCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف العقار بنجاح."));
    }
}
