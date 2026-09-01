namespace FalakAlkhair.Application.Common.Models;

/// <summary>
/// شكل موحّد لكل استجابات الـ API (نجاح أو فشل) حتى تكون الواجهة الأمامية
/// قادرة على التعامل مع كل الأخطاء بطريقة واحدة متوقّعة.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IReadOnlyList<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
