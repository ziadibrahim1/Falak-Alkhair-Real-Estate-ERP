using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Sales.Commands.CreateSale;
using FalakAlkhair.Application.Sales.Commands.UpdateSaleStage;
using FalakAlkhair.Application.Sales.Queries.GetSaleById;
using FalakAlkhair.Application.Sales.Queries.GetSalesList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة معاملات البيع عبر مسار مبيعات كامل (Sales Pipeline) مع توليد عمولة تلقائي عند الإتمام.</summary>
public class SalesController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.SaleView)]
    public async Task<IActionResult> GetList([FromQuery] GetSalesListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.SaleView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetSaleByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.SaleCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء معاملة البيع بنجاح."));
    }

    /// <summary>نقل معاملة البيع للمرحلة التالية ضمن المسار (أو الإلغاء).</summary>
    [HttpPost("{id:guid}/stage")]
    [Authorize(Policy = "Permission:" + Permissions.SaleManage)]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] UpdateSaleStageRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new UpdateSaleStageCommand { Id = id, Stage = request.Stage, CancellationReason = request.CancellationReason }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث مرحلة معاملة البيع."));
    }
}

public record UpdateSaleStageRequest(Domain.Common.Enums.SaleStage Stage, string? CancellationReason);
