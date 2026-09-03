using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Viewings.Commands.CompleteViewing;
using FalakAlkhair.Application.Viewings.Commands.CreateViewing;
using FalakAlkhair.Application.Viewings.Queries.GetViewingById;
using FalakAlkhair.Application.Viewings.Queries.GetViewingsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة معاينات العقارات/الوحدات للمشترين والمستأجرين المحتملين.</summary>
public class ViewingsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.ViewingView)]
    public async Task<IActionResult> GetList([FromQuery] GetViewingsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.ViewingView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetViewingByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.ViewingCreate)]
    public async Task<IActionResult> Create([FromBody] CreateViewingCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم جدولة المعاينة بنجاح."));
    }

    /// <summary>تسجيل نتيجة المعاينة: Completed/Cancelled/NoShow.</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "Permission:" + Permissions.ViewingEdit)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteViewingRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new CompleteViewingCommand { Id = id, Status = request.Status, Feedback = request.Feedback }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث نتيجة المعاينة."));
    }
}

public record CompleteViewingRequest(Domain.Common.Enums.ViewingStatus Status, string? Feedback);
