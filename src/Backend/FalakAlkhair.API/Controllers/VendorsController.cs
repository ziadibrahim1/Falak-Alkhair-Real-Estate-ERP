using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Vendors.Commands.CreateVendor;
using FalakAlkhair.Application.Vendors.Commands.DeleteVendor;
using FalakAlkhair.Application.Vendors.Commands.UpdateVendor;
using FalakAlkhair.Application.Vendors.Queries.GetVendorById;
using FalakAlkhair.Application.Vendors.Queries.GetVendorsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة شركات/موردي الصيانة الخارجيين.</summary>
public class VendorsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.VendorView)]
    public async Task<IActionResult> GetList([FromQuery] GetVendorsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.VendorView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetVendorByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.VendorCreate)]
    public async Task<IActionResult> Create([FromBody] CreateVendorCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة المورّد بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.VendorEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات المورّد بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.VendorDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteVendorCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف المورّد بنجاح."));
    }
}
