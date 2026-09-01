using FalakAlkhair.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Common.Behaviours;

/// <summary>
/// يشغّل كل FluentValidation Validators المسجّلة لكل Command/Query قبل
/// تنفيذ الـ Handler، ويرمي ValidationAppException موحّدة عند الفشل.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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
