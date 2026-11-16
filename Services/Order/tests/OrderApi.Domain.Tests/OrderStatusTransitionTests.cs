using OrderApi.Domain.Errors;
using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderStatusTransitionTests
{
    [Fact]
    public void HappyPath_PendingToDelivered()
    {
        var order = PendingOrder();

        Assert.True(order.Pay().IsSuccess);
        Assert.True(order.Ship().IsSuccess);
        Assert.True(order.Deliver().IsSuccess);

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Pay_WhenNotPending_ReturnsCannotPayAndKeepsStatus(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.Pay();

        Assert.Equal(DomainErrors.Order.CannotPay, result.Error);
        Assert.Equal(status, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Ship_WhenNotPaid_ReturnsCannotShipAndKeepsStatus(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.Ship();

        Assert.Equal(DomainErrors.Order.CannotShip, result.Error);
        Assert.Equal(status, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Deliver_WhenNotShipped_ReturnsCannotDeliverAndKeepsStatus(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.Deliver();

        Assert.Equal(DomainErrors.Order.CannotDeliver, result.Error);
        Assert.Equal(status, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Paid)]
    public void Cancel_WhenPendingOrPaid_CancelsOrder(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Cancel_WhenShippedDeliveredOrCancelled_ReturnsCannotCancelAndKeepsStatus(OrderStatus status)
    {
        var order = OrderWithStatus(status);

        var result = order.Cancel();

        Assert.Equal(DomainErrors.Order.CannotCancel, result.Error);
        Assert.Equal(status, order.Status);
    }
}
