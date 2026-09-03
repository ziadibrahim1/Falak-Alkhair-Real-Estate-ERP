using System.Text;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Common.Utilities;
using FalakAlkhair.Application.Reports.Queries.GetCommissionSummaryReport;
using FalakAlkhair.Application.Reports.Queries.GetMaintenanceSummaryReport;
using FalakAlkhair.Application.Reports.Queries.GetOccupancyReport;
using FalakAlkhair.Application.Reports.Queries.GetOwnerStatement;
using FalakAlkhair.Application.Reports.Queries.GetRentRollReport;
using FalakAlkhair.Application.Reports.Queries.GetSalesPipelineReport;
using FalakAlkhair.Application.Reports.Queries.GetTenantStatement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// التقارير المالية والتشغيلية. كشوف الحسابات (owner/tenant statement) وتقارير
/// Phase 8 التشغيلية (Rent Roll، مسار المبيعات، ملخص العمولات، ملخص الصيانة،
/// الإشغال) — كل تقرير متاح بصيغة JSON للعرض وCSV للتصدير (`/export`) دون أي
/// اعتمادية على مكتبة PDF/Excel خارجية (راجع CsvWriter في Application.Common.Utilities).
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

    [HttpGet("rent-roll")]
    [Authorize(Policy = "Permission:" + Permissions.ReportView)]
    public async Task<IActionResult> GetRentRoll(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetRentRollReportQuery(), cancellationToken)));

    [HttpGet("rent-roll/export")]
    [Authorize(Policy = "Permission:" + Permissions.ReportExport)]
    public async Task<IActionResult> ExportRentRoll(CancellationToken cancellationToken)
    {
        var rows = await Mediator.Send(new GetRentRollReportQuery(), cancellationToken);
        var csv = CsvWriter.Write(
            new[] { "رقم العقد", "العقار", "الوحدة", "المستأجر", "تاريخ البداية", "تاريخ النهاية", "الإيجار السنوي", "دورية السداد", "أقرب استحقاق" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[]
            {
                r.LeaseNumber, r.PropertyName, r.UnitNumber, r.TenantNameAr,
                r.StartDate.ToString("yyyy-MM-dd"), r.EndDate.ToString("yyyy-MM-dd"),
                r.AnnualRentAmount, r.PaymentFrequency, r.NextDueDate?.ToString("yyyy-MM-dd")
            }));
        return CsvFile(csv, "rent-roll.csv");
    }

    [HttpGet("sales-pipeline")]
    [Authorize(Policy = "Permission:" + Permissions.ReportView)]
    public async Task<IActionResult> GetSalesPipeline(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetSalesPipelineReportQuery(), cancellationToken)));

    [HttpGet("sales-pipeline/export")]
    [Authorize(Policy = "Permission:" + Permissions.ReportExport)]
    public async Task<IActionResult> ExportSalesPipeline(CancellationToken cancellationToken)
    {
        var rows = await Mediator.Send(new GetSalesPipelineReportQuery(), cancellationToken);
        var csv = CsvWriter.Write(
            new[] { "المرحلة", "عدد المعاملات", "إجمالي سعر الطلب" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[] { r.Stage, r.Count, r.TotalAskingValue }));
        return CsvFile(csv, "sales-pipeline.csv");
    }

    [HttpGet("commission-summary")]
    [Authorize(Policy = "Permission:" + Permissions.ReportView)]
    public async Task<IActionResult> GetCommissionSummary(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetCommissionSummaryReportQuery(), cancellationToken)));

    [HttpGet("commission-summary/export")]
    [Authorize(Policy = "Permission:" + Permissions.ReportExport)]
    public async Task<IActionResult> ExportCommissionSummary(CancellationToken cancellationToken)
    {
        var rows = await Mediator.Send(new GetCommissionSummaryReportQuery(), cancellationToken);
        var csv = CsvWriter.Write(
            new[] { "المسوّق", "عدد العمولات", "معلَّقة", "معتمدة", "مصروفة", "الإجمالي الصافي" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[]
                { r.AgentNameAr, r.CommissionsCount, r.PendingAmount, r.ApprovedAmount, r.PaidAmount, r.TotalNetAmount }));
        return CsvFile(csv, "commission-summary.csv");
    }

    [HttpGet("maintenance-summary")]
    [Authorize(Policy = "Permission:" + Permissions.ReportView)]
    public async Task<IActionResult> GetMaintenanceSummary(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetMaintenanceSummaryReportQuery(), cancellationToken)));

    [HttpGet("maintenance-summary/export")]
    [Authorize(Policy = "Permission:" + Permissions.ReportExport)]
    public async Task<IActionResult> ExportMaintenanceSummary(CancellationToken cancellationToken)
    {
        var rows = await Mediator.Send(new GetMaintenanceSummaryReportQuery(), cancellationToken);
        var csv = CsvWriter.Write(
            new[] { "الحالة", "العدد", "إجمالي التكلفة التقديرية", "إجمالي التكلفة الفعلية" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[] { r.Status, r.Count, r.TotalEstimatedCost, r.TotalActualCost }));
        return CsvFile(csv, "maintenance-summary.csv");
    }

    [HttpGet("occupancy")]
    [Authorize(Policy = "Permission:" + Permissions.ReportView)]
    public async Task<IActionResult> GetOccupancy(CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetOccupancyReportQuery(), cancellationToken)));

    [HttpGet("occupancy/export")]
    [Authorize(Policy = "Permission:" + Permissions.ReportExport)]
    public async Task<IActionResult> ExportOccupancy(CancellationToken cancellationToken)
    {
        var rows = await Mediator.Send(new GetOccupancyReportQuery(), cancellationToken);
        var csv = CsvWriter.Write(
            new[] { "العقار", "إجمالي الوحدات", "مؤجَّرة", "مباعة", "متاحة", "نسبة الإشغال %" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[]
                { r.PropertyName, r.TotalUnits, r.RentedUnits, r.SoldUnits, r.AvailableUnits, r.OccupancyRate }));
        return CsvFile(csv, "occupancy.csv");
    }

    private FileContentResult CsvFile(string csv, string fileName)
    {
        // BOM صريح (UTF-8) ليعرض إكسل النصوص العربية بشكل صحيح دون رموز مشوَّهة.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", fileName);
    }
}
