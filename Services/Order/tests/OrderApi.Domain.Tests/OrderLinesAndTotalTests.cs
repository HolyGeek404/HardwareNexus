using OrderApi.Domain.Entities;
using OrderApi.Domain.Errors;
using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderLinesAndTotalTests
{
    [Fact]
    public void Total_IsSumOfUnitPriceTimesQuantity()
    {
        var order = PendingOrder(
            Line(unitPrice: 100m, quantity: 3),
            Line(unitPrice: 50m, quantity: 1));

        Assert.Equal(Money.Create(350m, Currency.PLN), order.Total);
    }

    [Fact]
    public void AddOrderLine_UpdatesTotal()
    {
        var order = PendingOrder(Line(unitPrice: 100m, quantity: 1));

        var result = order.AddOrderLine(Line(unitPrice: 20m, quantity: 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(Money.Create(140m, Currency.PLN), order.Total);
    }

    [Fact]
    public void AddOrderLine_WithDifferentCurrency_ReturnsCurrencyMismatchError()
    {
        var order = PendingOrder(Line(currency: Currency.PLN));

        var result = order.AddOrderLine(Line(currency: Currency.EUR));

        Assert.Equal(DomainErrors.Order.CurrencyMismatch, result.Error);
    }

    [Fact]
    public void AddOrderLine_WithProductAlreadyInOrder_ReturnsDuplicateProductError()
    {
        var productId = Guid.NewGuid();
        var order = PendingOrder(Line(productId: productId));

        var result = order.AddOrderLine(Line(productId: productId));

        Assert.Equal(DomainErrors.Order.DuplicateProduct, result.Error);
    }

    [Fact]
    public void AddOrderLine_WhenRejected_DoesNotChangeOrder()
    {
        var productId = Guid.NewGuid();
        var order = PendingOrder(Line(unitPrice: 100m, productId: productId));

        order.AddOrderLine(Line(unitPrice: 999m, productId: productId));

        // Porażka nie może zostawić agregatu w połowie zmiany.
        Assert.Single(order.Lines);
        Assert.Equal(Money.Create(100m, Currency.PLN), order.Total);
    }

    [Theory]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void AddOrderLine_WhenOrderIsNotPending_ReturnsNotPendingError(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.AddOrderLine(Line());

        Assert.Equal(DomainErrors.Order.NotPending, result.Error);
    }

    [Fact]
    public void Lines_CannotBeModifiedFromOutside()
    {
        var order = PendingOrder();

        // Rzutowanie z powrotem na listę to klasyczny sposób na obejście enkapsulacji.
        Assert.False(order.Lines is List<OrderLine>);
    }
}
