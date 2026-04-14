using FluentValidation;
using MediatR;

namespace WeatherMonitor.Api.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validation = validators.Select(validator => validator.ValidateAsync(request, cancellationToken));

        var results = await Task.WhenAll(validation);

        var failures = results.SelectMany(result => result.Errors).ToArray();

        if (failures is { Length: > 0 })
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}