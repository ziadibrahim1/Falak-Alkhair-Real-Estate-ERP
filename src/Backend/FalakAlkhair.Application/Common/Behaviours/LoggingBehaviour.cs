using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FalakAlkhair.Application.Common.Behaviours;

/// <summary>يسجّل كل Command/Query عبر Structured Logging مع اسم المستخدم والشركة والمدة الزمنية.</summary>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "بدء تنفيذ {RequestName} بواسطة المستخدم {UserId} (الشركة {CompanyId})",
            requestName, _currentUserService.UserId, _currentUserService.CompanyId);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "اكتمل {RequestName} خلال {ElapsedMs}ms",
                requestName, (DateTime.UtcNow - startedAt).TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل تنفيذ {RequestName} بواسطة المستخدم {UserId}", requestName, _currentUserService.UserId);
            throw;
        }
    }
}
