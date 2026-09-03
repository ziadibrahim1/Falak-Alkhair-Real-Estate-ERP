using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Reports.Queries.GetOwnerStatement;
using FalakAlkhair.Application.Reports.Queries.GetTenantStatement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// التقارير المالية الأولية (كشوف الحسابات — البندان 40 و41). موديول التقارير
/// الكامل (طباعة PDF/Excel وبقية التقارير المذكورة في البند 38) مُخطَّط له في
/// Phase 8 حسب خارطة الطريق؛ هذا Controller يوفّر البيانات الحقيقية اللازمة له مبكرًا.
/// </summary>
[Route("api/reports")]
public class ReportsController : BaseApiController
{
    [HttpGet("owner-statement/{ownerId:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.FinancialView)]
    public async Task<IActionResult> GetOwnerStatement(Guid ownerId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOwnerStatementQuery(ownerId, from, to), cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("tenant-statement/{tenantId:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.TenantView)]
    public async Task<IActionResult> GetTenantStatement(Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTenantStatementQuery(tenantId), cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
