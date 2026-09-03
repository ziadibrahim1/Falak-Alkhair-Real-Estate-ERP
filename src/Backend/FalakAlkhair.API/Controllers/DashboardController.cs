using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إحصائيات لوحة التحكم المجمَّعة على الخادم بأمر واحد.</summary>
public class DashboardController : BaseApiController
{
    [HttpGet("stats")]
    [Authorize(Policy = "Permission:" + Permissions.DashboardView)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetDashboardStatsQuery(), cancellationToken)));
}
