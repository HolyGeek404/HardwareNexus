using FluentValidation;
using MediatR;
using OrderApi.Application.Behaviors;
using OrderApi.Application.Errors;
using OrderApi.Application.Features.Commands.PlaceOrder;
using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Tests;

public class ValidationBehaviorTests
{
    private static readonly PlaceOrderCommand InvalidCommand =
        new(Guid.Empty, [new PlaceOrderItem(Guid.Empty, 0)]);

    private static readonly PlaceOrderCommand ValidCommand =
        new(Guid.NewGuid(), [new PlaceOrderItem(Guid.NewGuid(), 1)]);

    private static ValidationBehavior<PlaceOrderCommand, Result<Guid>> Behavior(params IValidator<PlaceOrderCommand>[] validators) =>
        new(validators);

    [Fact]
    public async Task InvalidRequest_ReturnsValidationErrorWithAllFieldErrors_AndDoesNotCallHandler()
    {
        var handlerCalled = false;
        RequestHandlerDelegate<Result<Guid>> next = _ =>
        {
            handlerCalled = true;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        };

        var result = await Behavior(new PlaceOrderCommandValidator()).Handle(InvalidCommand, next, CancellationToken.None);

        Assert.False(handlerCalled);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        var validationError = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(3, validationError.Errors.Count); // CustomerId, Items[0].ProductId, Items[0].Quantity
    }

    [Fact]
    public async Task ValidRequest_PassesThroughToHandler()
    {
        var expectedId = Guid.NewGuid();
        RequestHandlerDelegate<Result<Guid>> next = _ => Task.FromResult(Result.Success(expectedId));

        var result = await Behavior(new PlaceOrderCommandValidator()).Handle(ValidCommand, next, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value);
    }

    [Fact]
    public async Task NoValidatorsRegistered_PassesThroughToHandler()
    {
        RequestHandlerDelegate<Result<Guid>> next = _ => Task.FromResult(Result.Success(Guid.NewGuid()));

        var result = await Behavior().Handle(InvalidCommand, next, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WorksForNonGenericResult()
    {
        // Ten sam behavior musi umieć zbudować porażkę dla zwykłego Result (bez wartości).
        var behavior = new ValidationBehavior<PlaceOrderCommand, Result>([new PlaceOrderCommandValidator()]);
        RequestHandlerDelegate<Result> next = _ => Task.FromResult(Result.Success());

        var result = await behavior.Handle(InvalidCommand, next, CancellationToken.None);

        Assert.IsType<ValidationError>(result.Error);
    }
}
