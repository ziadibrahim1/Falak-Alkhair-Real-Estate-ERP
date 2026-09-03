using FalakAlkhair.Application.Auctions.Commands.ProcessAuctionWebhookEvent;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Infrastructure.Integrations.Auctions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// نقطة الاستقبال الحقيقية لأحداث منصة المزادات المستقلة (Inbound Webhook) —
/// مصادقة بسرّ مشترك بدل JWT لأن المستدعي نظام خارجي وليس مستخدمًا داخل
/// النظام. كل حدث يُسجَّل في سجل تدقيق غير قابل للتعديل بغضّ النظر عن نوعه.
/// لا يرث BaseApiController عمدًا (يحمل [Route("api/[controller]")]) حتى لا
/// يُسجَّل مسار مزدوج بجانب المسار الصريح أدناه.
/// </summary>
[ApiController]
[Route("api/integrations/auctions")]
[AllowAnonymous]
public class AuctionWebhooksController : ControllerBase
{
    private readonly AuctionPlatformSettings _settings;
    private readonly ISender _mediator;

    public AuctionWebhooksController(IOptions<AuctionPlatformSettings> settings, ISender mediator)
    {
        _settings = settings.Value;
        _mediator = mediator;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveWebhook([FromBody] AuctionWebhookPayload payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            return StatusCode(503, ApiResponse<object>.Fail(
                "لم يتم تكوين السرّ المشترك لاستقبال أحداث منصة المزادات (AuctionIntegration:WebhookSecret)."));
        }

        if (!Request.Headers.TryGetValue("X-Auction-Webhook-Secret", out var providedSecret) ||
            providedSecret != _settings.WebhookSecret)
        {
            return Unauthorized(ApiResponse<object>.Fail("سرّ Webhook غير صحيح."));
        }

        await _mediator.Send(new ProcessAuctionWebhookEventCommand
        {
            ExternalAuctionId = payload.ExternalAuctionId,
            EventType = payload.EventType,
            BidAmount = payload.BidAmount,
            NewEndDate = payload.NewEndDate,
            OccurredAt = payload.OccurredAt,
            RawPayload = payload.Raw,
            SourceIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "تم استقبال الحدث."));
    }
}

public record AuctionWebhookPayload(
    string ExternalAuctionId,
    string EventType,
    decimal? BidAmount,
    DateTime? NewEndDate,
    DateTime? OccurredAt,
    string? Raw);
