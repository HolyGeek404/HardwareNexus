using OrderApi.Domain.Errors;
using OrderApi.Domain.Events;
using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class Order : AggregateRoot<OrderId>
{
    public DateTime OrderDate { get; private set; }
    public CustomerId CustomerId { get; private set; }
    private List<OrderLine> OrderLines { get; } = [];
    public IReadOnlyCollection<OrderLine> Lines => OrderLines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

#pragma warning disable CS8618
    private Order()
    {
    }
#pragma warning restore CS8618

    private Order(OrderId orderId ,CustomerId customer, DateTime orderDate, Money total) : base(orderId)
    {
        CustomerId = customer;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
        Total = total;
    }

    public static Result<Order> Create(OrderId orderId, CustomerId customer, DateTime orderDate, List<OrderLine> orderLines)
    {
        if (orderLines.Count == 0)
            return DomainErrors.Order.Empty;

        var total = Money.Create(0, orderLines[0].Price.Currency);
        var order = new Order(orderId, customer, orderDate, total);

        foreach (var orderLine in orderLines)
        {
            var result = order.AddOrderLine(orderLine);
            if (result.IsFailure)
                return result.Error;
        }

        order.Raise(new OrderPlacedDomainEvent(order.Id, order.CustomerId, order.Total, orderDate));

        return order;
    }

    public Result AddOrderLine(OrderLine orderLine)
    {
        if (Status != OrderStatus.Pending)
            return DomainErrors.Order.NotPending;

        if (OrderLines.Count != 0)
        {
            if (orderLine.Price.Currency != OrderLines[0].Price.Currency)
                return DomainErrors.Order.CurrencyMismatch;

            if (OrderLines.Contains(orderLine))
                return DomainErrors.Order.DuplicateProduct;
        }

        OrderLines.Add(orderLine);
        CalculateTotal(orderLine.TotalPrice);

        return Result.Success();
    }

    public Result Pay()
    {
        if (Status != OrderStatus.Pending)
            return DomainErrors.Order.CannotPay;

        Status = OrderStatus.Paid;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Paid))
            return DomainErrors.Order.CannotCancel;

        Status = OrderStatus.Cancelled;
        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != OrderStatus.Paid)
            return DomainErrors.Order.CannotShip;

        Status = OrderStatus.Shipped;
        return Result.Success();
    }

    public Result Deliver()
    {
        if (Status != OrderStatus.Shipped)
            return DomainErrors.Order.CannotDeliver;

        Status = OrderStatus.Delivered;
        return Result.Success();
    }

    private void CalculateTotal(Money orderLinePrice)
    {
       Total = Total.Add(orderLinePrice);
    }
}
