using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderStatusTransitionTests
{
    [Fact]
    public void HappyPath_PendingToDelivered()
    {
        var order = PendingOrder();

        order.Pay();
        order.Ship();
        order.Deliver();

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Pay_WhenNotPending_Throws(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(order.Pay);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Ship_WhenNotPaid_Throws(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(order.Ship);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Deliver_WhenNotShipped_Throws(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(order.Deliver);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Paid)]
    public void Cancel_WhenPendingOrPaid_CancelsOrder(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Cancel_WhenShippedDeliveredOrCancelled_Throws(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(order.Cancel);
    }
}
