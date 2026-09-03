using FalakAlkhair.Application.Agents.Commands.CreateAgent;
using FalakAlkhair.Application.Agents.Commands.DeleteAgent;
using FalakAlkhair.Application.Agents.Commands.UpdateAgent;
using FalakAlkhair.Application.Agents.Queries.GetAgentById;
using FalakAlkhair.Application.Agents.Queries.GetAgentsList;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة المسوّقين العقاريين (Real Estate Agents) وتراخيص فال.</summary>
public class AgentsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.AgentView)]
    public async Task<IActionResult> GetList([FromQuery] GetAgentsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AgentView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetAgentByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.AgentCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAgentCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة المسوّق العقاري بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AgentEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAgentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات المسوّق بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AgentDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteAgentCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف المسوّق بنجاح."));
    }
}
