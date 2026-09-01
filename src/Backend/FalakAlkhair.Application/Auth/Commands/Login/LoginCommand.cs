using FalakAlkhair.Application.Auth.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResponseDto>
{
    public string UserNameOrEmail { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? IpAddress { get; init; }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().WithMessage("اسم المستخدم أو البريد الإلكتروني مطلوب.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request.UserNameOrEmail, request.Password, request.IpAddress, cancellationToken);

        if (!result.Succeeded)
        {
            // رسالة عامة غير محدِّدة (لا نفصح إن كان الخطأ في اسم المستخدم أو كلمة المرور) لمنع Account Enumeration.
            throw new ForbiddenAccessException("بيانات الدخول غير صحيحة أو الحساب موقوف.");
        }

        return new AuthResponseDto
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            AccessTokenExpiresAt = result.AccessTokenExpiresAt!.Value
        };
    }
}
