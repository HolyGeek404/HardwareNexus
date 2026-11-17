using System.Reflection;
using FluentValidation;
using MediatR;
using OrderApi.Application.Errors;
using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToList();

        if (errors.Count == 0)
            return await next(cancellationToken);

        return CreateFailure(new ValidationError(errors));
    }
    
    private static readonly MethodInfo? GenericFailure =
        typeof(TResponse).IsGenericType
            ? typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0])
            : null;

    private static TResponse CreateFailure(Error error)
    {
        if (GenericFailure is null)
            return (TResponse)(object)Result.Failure(error);

        return (TResponse)GenericFailure.Invoke(null, [error])!;
    }
}
