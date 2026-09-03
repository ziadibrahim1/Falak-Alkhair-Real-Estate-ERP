using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Offers.Commands.CreateOffer;
using FalakAlkhair.Application.Offers.Commands.UpdateOfferStatus;
using FalakAlkhair.Application.Offers.Queries.GetOfferById;
using FalakAlkhair.Application.Offers.Queries.GetOffersList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة عروض الشراء المقدَّمة من المشترين المحتملين (يدعم تعدُّد العروض على نفس الوحدة).</summary>
public class OffersController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.OfferView)]
    public async Task<IActionResult> GetList([FromQuery] GetOffersListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.OfferView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetOfferByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.OfferCreate)]
    public async Task<IActionResult> Create([FromBody] CreateOfferCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم تسجيل عرض الشراء بنجاح."));
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Permission:" + Permissions.OfferEdit)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOfferStatusRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new UpdateOfferStatusCommand { Id = id, Status = request.Status }, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث حالة العرض."));
    }
}

public record UpdateOfferStatusRequest(Domain.Common.Enums.OfferStatus Status);
