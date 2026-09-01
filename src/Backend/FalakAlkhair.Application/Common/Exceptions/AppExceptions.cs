namespace FalakAlkhair.Application.Common.Exceptions;

/// <summary>يُرمى عند عدم العثور على كيان مطلوب. يُترجَم في API إلى 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"لم يتم العثور على \"{name}\" ({key}).")
    {
    }
}

/// <summary>يُرمى عند فشل قواعد التحقق (FluentValidation). يُترجَم في API إلى 400.</summary>
public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException()
        : base("حدث خطأ أو أكثر أثناء التحقق من صحة البيانات.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationAppException(IEnumerable<FluentValidation.Results.ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

/// <summary>يُرمى عندما لا يملك المستخدم الصلاحية اللازمة. يُترجَم في API إلى 403.</summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message = null)
        : base(message ?? "لا تملك الصلاحية الكافية لتنفيذ هذا الإجراء.")
    {
    }
}

/// <summary>يُرمى عند انتهاك قاعدة عمل (Business Rule)، مثال: محاولة اعتماد عقد منتهي.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
