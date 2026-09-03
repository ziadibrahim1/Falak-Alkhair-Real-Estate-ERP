using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceRequests.Commands.AssignMaintenanceRequest;
using FalakAlkhair.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;
using FalakAlkhair.Application.MaintenanceRequests.Commands.DeleteMaintenanceRequest;
using FalakAlkhair.Application.MaintenanceRequests.Commands.UpdateMaintenanceStatus;
using FalakAlkhair.Application.MaintenanceRequests.Queries.GetMaintenanceRequestById;
using FalakAlkhair.Application.MaintenanceRequests.Queries.GetMaintenanceRequestsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة طلبات الصيانة عبر دورة عمل كاملة من الإنشاء حتى الإكمال.</summary>
public class MaintenanceRequestsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestView)]
    public async Task<IActionResult> GetList([FromQuery] GetMaintenanceRequestsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetMaintenanceRequestByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestCreate)]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء طلب الصيانة بنجاح."));
    }

    /// <summary>إسناد الطلب لفني داخلي و/أو مورّد خارجي.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestAssign)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignMaintenanceRequestRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new AssignMaintenanceRequestCommand { Id = id, EmployeeId = request.EmployeeId, VendorId = request.VendorId }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم إسناد طلب الصيانة."));
    }

    /// <summary>تحديث حالة الطلب ضمن دورة العمل (تقدُّم للأمام فقط، عدا الإلغاء).</summary>
    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestEdit)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new UpdateMaintenanceStatusCommand { Id = id, Status = request.Status, ActualCost = request.ActualCost }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث حالة طلب الصيانة."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.MaintenanceRequestDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteMaintenanceRequestCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف طلب الصيانة بنجاح."));
    }
}

public record AssignMaintenanceRequestRequest(Guid? EmployeeId, Guid? VendorId);
public record UpdateMaintenanceStatusRequest(Domain.Common.Enums.MaintenanceStatus Status, decimal? ActualCost);
