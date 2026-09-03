using FalakAlkhair.Application.Commissions.Commands.CreateCommission;
using FalakAlkhair.Application.Commissions.Commands.MarkCommissionPaid;
using FalakAlkhair.Application.Commissions.Queries.GetCommissionById;
using FalakAlkhair.Application.Commissions.Queries.GetCommissionsList;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// عمولات المسوّقين العقاريين. أغلب العمولات تُولَّد تلقائيًا عند تفعيل عقود
/// الإيجار (راجع LeasesController/activate)؛ هذا الـ Controller يخدم العرض،
/// التسجيل اليدوي الاستثنائي، وتسجيل الصرف.
/// </summary>
public class CommissionsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.CommissionView)]
    public async Task<IActionResult> GetList([FromQuery] GetCommissionsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.CommissionView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetCommissionByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.CommissionManage)]
    public async Task<IActionResult> Create([FromBody] CreateCommissionCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم تسجيل العمولة بنجاح."));
    }

    /// <summary>تسجيل صرف العمولة للمسوّق.</summary>
    [HttpPost("{id:guid}/mark-paid")]
    [Authorize(Policy = "Permission:" + Permissions.CommissionManage)]
    public async Task<IActionResult> MarkPaid(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new MarkCommissionPaidCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تسجيل صرف العمولة."));
    }
}
