using FalakAlkhair.Application.Buyers.Commands.CreateBuyer;
using FalakAlkhair.Application.Buyers.Commands.DeleteBuyer;
using FalakAlkhair.Application.Buyers.Commands.UpdateBuyer;
using FalakAlkhair.Application.Buyers.Queries.GetBuyerById;
using FalakAlkhair.Application.Buyers.Queries.GetBuyerMatches;
using FalakAlkhair.Application.Buyers.Queries.GetBuyersList;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة المشترين (Buyers CRM) ومحرك المطابقة البسيط مع العقارات المعروضة للبيع.</summary>
public class BuyersController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.BuyerView)]
    public async Task<IActionResult> GetList([FromQuery] GetBuyersListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.BuyerView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetBuyerByIdQuery(id), cancellationToken)));

    /// <summary>محرك مطابقة بسيط: يعيد الوحدات المعروضة للبيع المطابقة لمعايير المشتري.</summary>
    [HttpGet("{id:guid}/matches")]
    [Authorize(Policy = "Permission:" + Permissions.BuyerView)]
    public async Task<IActionResult> GetMatches(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetBuyerMatchesQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.BuyerCreate)]
    public async Task<IActionResult> Create([FromBody] CreateBuyerCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة المشتري بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.BuyerEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuyerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات المشتري بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.BuyerDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteBuyerCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف المشتري بنجاح."));
    }
}
