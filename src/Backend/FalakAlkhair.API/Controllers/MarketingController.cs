using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Marketing.Commands.CreateCampaign;
using FalakAlkhair.Application.Marketing.Commands.DeleteCampaign;
using FalakAlkhair.Application.Marketing.Commands.UpdateCampaign;
using FalakAlkhair.Application.Marketing.Queries.GetCampaignById;
using FalakAlkhair.Application.Marketing.Queries.GetCampaignsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة الحملات التسويقية العقارية. الأداء (Leads/Conversions) يُحسب من بيانات حقيقية.</summary>
public class MarketingController : BaseApiController
{
    [HttpGet("campaigns")]
    [Authorize(Policy = "Permission:" + Permissions.MarketingView)]
    public async Task<IActionResult> GetList([FromQuery] GetCampaignsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("campaigns/{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MarketingView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetCampaignByIdQuery(id), cancellationToken)));

    [HttpPost("campaigns")]
    [Authorize(Policy = "Permission:" + Permissions.MarketingCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء الحملة التسويقية بنجاح."));
    }

    [HttpPut("campaigns/{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MarketingEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث الحملة بنجاح."));
    }

    [HttpDelete("campaigns/{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MarketingDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCampaignCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف الحملة بنجاح."));
    }
}
