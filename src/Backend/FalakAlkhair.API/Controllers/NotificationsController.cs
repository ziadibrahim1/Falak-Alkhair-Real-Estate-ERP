using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Notifications.Commands.MarkAllNotificationsRead;
using FalakAlkhair.Application.Notifications.Commands.MarkNotificationRead;
using FalakAlkhair.Application.Notifications.Queries.GetMyNotifications;
using FalakAlkhair.Application.Notifications.Queries.GetUnreadNotificationCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إشعارات المستخدم الحالي (الموجَّهة له + العامة على مستوى الشركة).</summary>
public class NotificationsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.NotificationView)]
    public async Task<IActionResult> GetList([FromQuery] GetMyNotificationsQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("unread-count")]
    [Authorize(Policy = "Permission:" + Permissions.NotificationView)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        => Ok(ApiResponse<int>.Ok(await Mediator.Send(new GetUnreadNotificationCountQuery(), cancellationToken)));

    [HttpPost("{id:guid}/mark-read")]
    [Authorize(Policy = "Permission:" + Permissions.NotificationView)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديد الإشعار كمقروء."));
    }

    [HttpPost("mark-all-read")]
    [Authorize(Policy = "Permission:" + Permissions.NotificationView)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await Mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديد كل الإشعارات كمقروءة."));
    }
}
