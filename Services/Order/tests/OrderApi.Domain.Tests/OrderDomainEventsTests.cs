using OrderApi.Domain.Events;
using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class OrderDomainEventsTests
{
    [Fact]
    public void Create_WhenSuccessful_RaisesOrderPlacedWithOrderData()
    {
        var order = CreateOrder([Line(unitPrice: 100m, quantity: 2)]).Value;

        var placed = Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, placed.OrderId);
        Assert.Equal(order.CustomerId, placed.CustomerId);
        Assert.Equal(Money.Create(200m, Currency.PLN), placed.Total);
        Assert.Equal(OrderDate, placed.OccurredOnUtc);
    }

    [Fact]
    public void ClearDomainEvents_RemovesCollectedEvents()
    {
        var order = PendingOrder();

        order.ClearDomainEvents();

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void FailedOperation_DoesNotRaiseNewEvents()
    {
        var order = PendingOrder();
        order.ClearDomainEvents();

        order.Ship();

        Assert.Empty(order.DomainEvents);
    }
}
