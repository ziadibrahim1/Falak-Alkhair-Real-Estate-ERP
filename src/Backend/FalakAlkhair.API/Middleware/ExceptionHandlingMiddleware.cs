using System.Net;
using System.Text.Json;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Models;

namespace FalakAlkhair.API.Middleware;

/// <summary>
/// معالج استثناءات موحّد لكل الـ API: يحوّل كل استثناء إلى ApiResponse موحّد
/// مع كود HTTP مناسب، ولا يُظهر أبدًا Stack Trace للمستخدم النهائي. تفاصيل
/// الخطأ الكاملة تُسجَّل عبر Serilog فقط.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var (statusCode, response) = exception switch
        {
            ValidationAppException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("فشل التحقق من صحة البيانات.", validationEx.Errors.SelectMany(e => e.Value).ToList())),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message)),

            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(forbiddenEx.Message)),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse<object>.Fail(businessEx.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("يجب تسجيل الدخول للوصول لهذا المورد.")),

            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("حدث خطأ غير متوقع في الخادم. تم إبلاغ فريق الدعم الفني."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "خطأ غير متوقع أثناء معالجة الطلب {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
