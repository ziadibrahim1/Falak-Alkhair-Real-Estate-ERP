namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>تجريد للوقت الحالي لتسهيل الاختبار (Unit Testing) بدل DateTime.UtcNow المباشر.</summary>
public interface IDateTime
{
    DateTime Now { get; }
}
