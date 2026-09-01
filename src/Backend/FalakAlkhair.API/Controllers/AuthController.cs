using FalakAlkhair.Application.Auth.Commands.Login;
using FalakAlkhair.Application.Auth.Commands.RefreshToken;
using FalakAlkhair.Application.Auth.Commands.RegisterUser;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FalakAlkhair.API.Controllers;

[EnableRateLimiting("AuthPolicy")]
public class AuthController : BaseApiController
{
    /// <summary>تسجيل الدخول وإصدار Access Token + Refresh Token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LoginCommand
        {
            UserNameOrEmail = request.UserNameOrEmail,
            Password = request.Password,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result, "تم تسجيل الدخول بنجاح."));
    }

    /// <summary>تحديث Access Token باستخدام Refresh Token صالح (Token Rotation).</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>إنشاء مستخدم جديد — يتطلب صلاحية User.Manage.</summary>
    [HttpPost("register")]
    [Authorize(Policy = "Permission:" + Permissions.UserManage)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<Guid>.Ok(id, "تم إنشاء المستخدم بنجاح."));
    }
}

public record LoginRequest(string UserNameOrEmail, string Password);
public record RefreshTokenRequest(string RefreshToken);
