using FalakAlkhair.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Common.Behaviours;

/// <summary>
/// يشغّل كل FluentValidation Validators المسجّلة لكل Command/Query قبل
/// تنفيذ الـ Handler، ويرمي ValidationAppException موحّدة عند الفشل.
/// القيد هنا عمدًا "notnull" وليس "IRequest&lt;TResponse&gt;": في MediatR 12.x
/// الواجهة غير المعمَّمة IRequest لا ترث IRequest&lt;Unit&gt; (خلافًا لإصدارات
/// أقدم)، فلو قُيِّد TRequest بـ IRequest&lt;TResponse&gt; هنا لَفشل تسجيل هذا
/// الـ Behavior صامتًا (Silent No-Op) لكل أمر بلا نتيجة (IRequest، مثل الاعتماد/
/// الإلغاء/التحديث)، فتُتخطى كل قواعد FluentValidation الخاصة به دون أي خطأ
/// ظاهر — وهو ما وقع فعليًا هنا حتى اكتُشف واختُبر بمعزل عن بقية النظام.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationAppException(failures);
        }

        return await next();
    }
}
