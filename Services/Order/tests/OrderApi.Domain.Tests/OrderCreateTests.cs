using OrderApi.Domain.Entities;
using OrderApi.Domain.Tests.TestData;
using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderCreateTests
{
    private static Order CreateOrder(List<OrderLine> lines) =>
        Order.Create(
            OrderId.Create(Guid.NewGuid()),
            CustomerId.Create(Guid.NewGuid()),
            OrderMother.OrderDate,
            lines);

    [Fact]
    public void Create_WithValidLines_ReturnsPendingOrderWithAllLines()
    {
        var order = CreateOrder([Line(), Line()]);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => CreateOrder([]));
    }

    [Fact]
    public void Create_WithSameProductTwice_Throws()
    {
        var productId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() =>
            CreateOrder([Line(productId: productId), Line(productId: productId)]));
    }

    [Fact]
    public void Create_WithMixedCurrencies_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CreateOrder([Line(currency: Currency.PLN), Line(currency: Currency.EUR)]));
    }

    [Fact]
    public void Create_WhenCallerModifiesListAfterwards_OrderIsUnaffected()
    {
        var lines = new List<OrderLine> { Line() };
        var order = CreateOrder(lines);

        lines.Clear();

        Assert.Single(order.Lines);
    }
}
