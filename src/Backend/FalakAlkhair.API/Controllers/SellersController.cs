using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Sellers.Commands.CreateSeller;
using FalakAlkhair.Application.Sellers.Commands.DeleteSeller;
using FalakAlkhair.Application.Sellers.Commands.UpdateSeller;
using FalakAlkhair.Application.Sellers.Queries.GetSellerById;
using FalakAlkhair.Application.Sellers.Queries.GetSellersList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة البائعين وتفويضات البيع (Sale Mandates).</summary>
public class SellersController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.SellerView)]
    public async Task<IActionResult> GetList([FromQuery] GetSellersListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.SellerView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetSellerByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.SellerCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSellerCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة البائع (تفويض البيع) بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.SellerEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSellerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات البائع بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.SellerDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteSellerCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف البائع بنجاح."));
    }
}
