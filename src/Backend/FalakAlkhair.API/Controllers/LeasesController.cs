using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Leases.Commands.ActivateLease;
using FalakAlkhair.Application.Leases.Commands.CreateLease;
using FalakAlkhair.Application.Leases.Commands.TerminateLease;
using FalakAlkhair.Application.Leases.Commands.UpdateLease;
using FalakAlkhair.Application.Leases.Queries.GetLeaseById;
using FalakAlkhair.Application.Leases.Queries.GetLeasesList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة عقود الإيجار (Lease Management) مع جدول سداد تلقائي وWorkflow التفعيل/الإنهاء.</summary>
public class LeasesController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.LeaseView)]
    public async Task<IActionResult> GetList([FromQuery] GetLeasesListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.LeaseView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetLeaseByIdQuery(id), cancellationToken)));

    /// <summary>إنشاء عقد إيجار جديد (Draft) مع توليد جدول السداد تلقائيًا.</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.LeaseCreate)]
    public async Task<IActionResult> Create([FromBody] CreateLeaseCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء عقد الإيجار بنجاح (مسودة) مع جدول السداد."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.LeaseEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث العقد بنجاح."));
    }

    /// <summary>تفعيل العقد: Draft/PendingApproval → Active.</summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "Permission:" + Permissions.LeaseActivate)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ActivateLeaseCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تفعيل عقد الإيجار."));
    }

    /// <summary>إنهاء عقد نشط: Active → Terminated.</summary>
    [HttpPost("{id:guid}/terminate")]
    [Authorize(Policy = "Permission:" + Permissions.LeaseTerminate)]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateLeaseRequest? request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new TerminateLeaseCommand(id, request?.Reason), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم إنهاء عقد الإيجار."));
    }
}

public record TerminateLeaseRequest(string? Reason);
