using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Listings.Commands.CreateListing;
using FalakAlkhair.Application.Listings.Commands.DeleteListing;
using FalakAlkhair.Application.Listings.Commands.PublishListing;
using FalakAlkhair.Application.Listings.Commands.UpdateListing;
using FalakAlkhair.Application.Listings.Queries.GetListingById;
using FalakAlkhair.Application.Listings.Queries.GetListingsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة الإعلانات العقارية (Listings) مع منع نشر إعلان بلا بيانات كافية.</summary>
public class ListingsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.ListingView)]
    public async Task<IActionResult> GetList([FromQuery] GetListingsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.ListingView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetListingByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.ListingCreate)]
    public async Task<IActionResult> Create([FromBody] CreateListingCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء الإعلان العقاري بنجاح (مسودة)."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.ListingEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم تحديث الإعلان بنجاح."));
    }

    /// <summary>نشر الإعلان — يتحقق من اكتمال البيانات المطلوبة ويحدّث حالة الوحدة.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "Permission:" + Permissions.ListingPublish)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new PublishListingCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم نشر الإعلان."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.ListingDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteListingCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف الإعلان بنجاح."));
    }
}
