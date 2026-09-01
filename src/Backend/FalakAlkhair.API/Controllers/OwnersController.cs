using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Owners.Commands.CreateOwner;
using FalakAlkhair.Application.Owners.Commands.DeleteOwner;
using FalakAlkhair.Application.Owners.Commands.UpdateOwner;
using FalakAlkhair.Application.Owners.Queries.GetOwnerById;
using FalakAlkhair.Application.Owners.Queries.GetOwnersList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة أصحاب الأملاك (Owners CRM).</summary>
public class OwnersController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.OwnerView)]
    public async Task<IActionResult> GetList([FromQuery] GetOwnersListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.OwnerView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetOwnerByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.OwnerCreate)]
    public async Task<IActionResult> Create([FromBody] CreateOwnerCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء المالك بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.OwnerEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOwnerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم تحديث بيانات المالك بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.OwnerDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteOwnerCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم حذف المالك بنجاح."));
    }
}
