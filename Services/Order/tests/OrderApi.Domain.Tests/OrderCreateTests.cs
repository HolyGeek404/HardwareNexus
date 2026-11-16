using OrderApi.Domain.Entities;
using OrderApi.Domain.Errors;
using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderCreateTests
{
    [Fact]
    public void Create_WithValidLines_ReturnsPendingOrderWithAllLines()
    {
        var result = CreateOrder([Line(), Line()]);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
        Assert.Equal(2, result.Value.Lines.Count);
    }

    [Fact]
    public void Create_WithNoLines_ReturnsEmptyError()
    {
        var result = CreateOrder([]);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Order.Empty, result.Error);
    }

    [Fact]
    public void Create_WithSameProductTwice_ReturnsDuplicateProductError()
    {
        var productId = Guid.NewGuid();

        var result = CreateOrder([Line(productId: productId), Line(productId: productId)]);

        Assert.Equal(DomainErrors.Order.DuplicateProduct, result.Error);
    }

    [Fact]
    public void Create_WithMixedCurrencies_ReturnsCurrencyMismatchError()
    {
        var result = CreateOrder([Line(currency: Currency.PLN), Line(currency: Currency.EUR)]);

        Assert.Equal(DomainErrors.Order.CurrencyMismatch, result.Error);
    }

    [Fact]
    public void Create_WhenCallerModifiesListAfterwards_OrderIsUnaffected()
    {
        var lines = new List<OrderLine> { Line() };
        var order = CreateOrder(lines).Value;

        lines.Clear();

        Assert.Single(order.Lines);
    }
}
