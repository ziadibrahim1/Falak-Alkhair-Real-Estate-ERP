using FalakAlkhair.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));
}
