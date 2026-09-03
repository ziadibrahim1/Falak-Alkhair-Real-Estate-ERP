using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.MaintenanceQuotations.Commands.ApproveQuotation;
using FalakAlkhair.Application.MaintenanceQuotations.Commands.CreateQuotation;
using FalakAlkhair.Application.MaintenanceQuotations.Queries.GetQuotationById;
using FalakAlkhair.Application.MaintenanceQuotations.Queries.GetQuotationsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>عروض أسعار الصيانة — تدعم تعدُّد العروض على نفس الطلب للمقارنة.</summary>
public class MaintenanceQuotationsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.QuotationView)]
    public async Task<IActionResult> GetList([FromQuery] GetQuotationsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.QuotationView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetQuotationByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.QuotationCreate)]
    public async Task<IActionResult> Create([FromBody] CreateQuotationCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم تسجيل عرض السعر بنجاح."));
    }

    /// <summary>اعتماد عرض سعر — يرفض بقية العروض المعلَّقة على نفس الطلب تلقائيًا.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Permission:" + Permissions.QuotationApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ApproveQuotationCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم اعتماد عرض السعر."));
    }
}
