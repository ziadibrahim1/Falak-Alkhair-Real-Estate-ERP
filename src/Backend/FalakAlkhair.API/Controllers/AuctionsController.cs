using FalakAlkhair.Application.Auctions.Commands.ApproveAuction;
using FalakAlkhair.Application.Auctions.Commands.AwardAuction;
using FalakAlkhair.Application.Auctions.Commands.CreateAuction;
using FalakAlkhair.Application.Auctions.Commands.PublishAuction;
using FalakAlkhair.Application.Auctions.Commands.SettleAuction;
using FalakAlkhair.Application.Auctions.Commands.UpdateAuctionStatus;
using FalakAlkhair.Application.Auctions.Queries.GetAuctionAuditLogs;
using FalakAlkhair.Application.Auctions.Queries.GetAuctionById;
using FalakAlkhair.Application.Auctions.Queries.GetAuctionsList;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// إدارة بيانات المزادات الأساسية (Master Data) داخل الـ ERP. المزايدة الحية
/// الفعلية تجري على منصة المزادات المستقلة؛ راجع AuctionWebhooksController
/// لنقطة استقبال أحداثها، وIAuctionPlatformClient لجهة الاتصال الصادرة.
/// </summary>
public class AuctionsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.AuctionView)]
    public async Task<IActionResult> GetList([FromQuery] GetAuctionsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetAuctionByIdQuery(id), cancellationToken)));

    /// <summary>سجل تدقيق المزاد الكامل (غير قابل للتعديل).</summary>
    [HttpGet("{id:guid}/audit-log")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionView)]
    public async Task<IActionResult> GetAuditLog(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetAuctionAuditLogsQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.AuctionCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAuctionCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء المزاد بنجاح (مسودة)."));
    }

    /// <summary>اعتماد المزاد: Draft/PendingApproval → Scheduled.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ApproveAuctionCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم اعتماد المزاد."));
    }

    /// <summary>نشر المزاد (داخليًا + مزامنة بأفضل جهد مع المنصة الخارجية): Scheduled → Published.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionApprove)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new PublishAuctionCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم نشر المزاد."));
    }

    /// <summary>تحديث الحالة يدويًا (PendingApproval/Live/Ended/Cancelled).</summary>
    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionManage)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAuctionStatusRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new UpdateAuctionStatusCommand { Id = id, Status = request.Status, CancellationReason = request.CancellationReason }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث حالة المزاد."));
    }

    /// <summary>إرساء المزاد على فائز: Ended → Awarded (يولّد عمولة تلقائيًا).</summary>
    [HttpPost("{id:guid}/award")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionManage)]
    public async Task<IActionResult> Award(Guid id, [FromBody] AwardAuctionRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new AwardAuctionCommand { Id = id, WinnerBuyerId = request.WinnerBuyerId, FinalPrice = request.FinalPrice }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم إرساء المزاد."));
    }

    /// <summary>تسوية مالية نهائية: Awarded → Settled.</summary>
    [HttpPost("{id:guid}/settle")]
    [Authorize(Policy = "Permission:" + Permissions.AuctionManage)]
    public async Task<IActionResult> Settle(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new SettleAuctionCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تمت تسوية المزاد."));
    }
}

public record UpdateAuctionStatusRequest(Domain.Common.Enums.AuctionStatus Status, string? CancellationReason);
public record AwardAuctionRequest(Guid WinnerBuyerId, decimal FinalPrice);
