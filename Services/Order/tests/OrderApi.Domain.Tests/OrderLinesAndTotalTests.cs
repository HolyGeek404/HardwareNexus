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

        order.AddOrderLine(Line(unitPrice: 20m, quantity: 2));

        Assert.Equal(Money.Create(140m, Currency.PLN), order.Total);
    }

    [Fact]
    public void AddOrderLine_WithDifferentCurrency_Throws()
    {
        var order = PendingOrder(Line(currency: Currency.PLN));

        Assert.Throws<InvalidOperationException>(() =>
            order.AddOrderLine(Line(currency: Currency.EUR)));
    }

    [Fact]
    public void AddOrderLine_WithProductAlreadyInOrder_Throws()
    {
        var productId = Guid.NewGuid();
        var order = PendingOrder(Line(productId: productId));

        Assert.Throws<InvalidOperationException>(() =>
            order.AddOrderLine(Line(productId: productId)));
    }

    [Theory]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void AddOrderLine_WhenOrderIsNotPending_Throws(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(() => order.AddOrderLine(Line()));
    }

    [Fact]
    public void Lines_CannotBeModifiedFromOutside()
    {
        var order = PendingOrder();

        // Rzutowanie z powrotem na listę to klasyczny sposób na obejście enkapsulacji.
        Assert.False(order.Lines is List<Entities.OrderLine>);
    }
}
