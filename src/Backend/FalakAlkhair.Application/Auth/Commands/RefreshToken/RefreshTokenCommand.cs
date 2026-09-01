using FalakAlkhair.Application.Auth.DTOs;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using MediatR;

namespace FalakAlkhair.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string RefreshToken { get; init; } = default!;
    public string? IpAddress { get; init; }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshTokenAsync(request.RefreshToken, request.IpAddress, cancellationToken);

        if (!result.Succeeded)
        {
            throw new ForbiddenAccessException("رمز التحديث غير صالح أو منتهي الصلاحية.");
        }

        return new AuthResponseDto
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            AccessTokenExpiresAt = result.AccessTokenExpiresAt!.Value
        };
    }
}
