using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Leads.Commands.AssignLead;
using FalakAlkhair.Application.Leads.Commands.CreateLead;
using FalakAlkhair.Application.Leads.Commands.DeleteLead;
using FalakAlkhair.Application.Leads.Commands.UpdateLead;
using FalakAlkhair.Application.Leads.Queries.GetLeadById;
using FalakAlkhair.Application.Leads.Queries.GetLeadsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة العملاء المحتملين (Leads) — نقطة الدخول المركزية لـ CRM النظام.</summary>
public class LeadsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.LeadView)]
    public async Task<IActionResult> GetList([FromQuery] GetLeadsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.LeadView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetLeadByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.LeadCreate)]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إضافة العميل المحتمل بنجاح."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.LeadEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث بيانات العميل المحتمل بنجاح."));
    }

    /// <summary>إسناد عميل محتمل لمسوّق عقاري.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "Permission:" + Permissions.LeadAssign)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignLeadRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new AssignLeadCommand { Id = id, AgentId = request.AgentId }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم إسناد العميل المحتمل للمسوّق."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.LeadDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteLeadCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف العميل المحتمل بنجاح."));
    }
}

public record AssignLeadRequest(Guid AgentId);
