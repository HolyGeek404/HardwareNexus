using OrderApi.Application.Features.Commands.PlaceOrder;

namespace OrderApi.Application.Tests;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    private static PlaceOrderItem ValidItem() => new(Guid.NewGuid(), 1);

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(), [ValidItem(), ValidItem()]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyCustomerId_Fails()
    {
        var result = _validator.Validate(new PlaceOrderCommand(Guid.Empty, [ValidItem()]));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlaceOrderCommand.CustomerId));
    }

    [Fact]
    public void NoItems_Fails()
    {
        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(), []));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlaceOrderCommand.Items));
    }

    [Fact]
    public void TooManyItems_Fails()
    {
        var items = Enumerable.Range(0, PlaceOrderCommandValidator.MaxItems + 1).Select(_ => ValidItem()).ToList();

        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(), items));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ItemWithEmptyProductId_FailsWithIndexInPropertyName()
    {
        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(), [ValidItem(), new PlaceOrderItem(Guid.Empty, 1)]));

        // FluentValidation podaje ścieżkę z indeksem – klient wie, która pozycja jest zła.
        Assert.Contains(result.Errors, e => e.PropertyName == "Items[1].ProductId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ItemWithNonPositiveQuantity_Fails(int quantity)
    {
        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(), [new PlaceOrderItem(Guid.NewGuid(), quantity)]));

        Assert.Contains(result.Errors, e => e.PropertyName == "Items[0].Quantity");
    }

    [Fact]
    public void DuplicateProducts_AreNotRejectedHere()
    {
        // Duplikaty to reguła biznesowa agregatu (Conflict), nie walidacja wejścia.
        var productId = Guid.NewGuid();

        var result = _validator.Validate(new PlaceOrderCommand(Guid.NewGuid(),
            [new PlaceOrderItem(productId, 1), new PlaceOrderItem(productId, 2)]));

        Assert.True(result.IsValid);
    }
}
