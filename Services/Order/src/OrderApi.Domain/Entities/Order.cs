using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class Order :  Entity<OrderId>
{
    public DateTime OrderDate { get; private set; }
    public CustomerId CustomerId { get; private set; }
    private List<OrderLine> OrderLines { get; } = [];
    public IReadOnlyCollection<OrderLine> Lines => OrderLines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

    private Order(OrderId orderId ,CustomerId customer, DateTime orderDate, Money total) : base(orderId)
    {
        CustomerId = customer;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
        Total = total;
    }
    public static Order Create(OrderId orderId, CustomerId customer, DateTime orderDate, List<OrderLine> orderLines)
    {
        if (orderLines.Count == 0)
            throw new InvalidOperationException("Order cant be empty");
        
        var total = Money.Create(0, orderLines[0].Price.Currency);
        var order = new Order(orderId, customer, orderDate, total);

        foreach (var orderLine in orderLines)
        {
            order.AddOrderLine(orderLine);
        }

        return order;
    }

    public void AddOrderLine(OrderLine orderLine)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cant add order line when order is not pending");
        
        if (OrderLines.Count != 0) 
        {
            if (orderLine.Price.Currency != OrderLines[0].Price.Currency)
                throw new InvalidOperationException("Cant add order line. Currency missmatch");
            
            if(OrderLines.Contains(orderLine))
                throw new InvalidOperationException("Duplicated order line");
        }
        OrderLines.Add(orderLine);
        CalculateTotal(orderLine.TotalPrice);
    }
    
    public void Pay()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order cant be paied");
        Status = OrderStatus.Paid;
    }
    
    public void Cancel()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Delivered or  OrderStatus.Shipped)
            throw new InvalidOperationException("Order cant be cancelled");
        Status = OrderStatus.Cancelled;
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Order cant be shipped");
        Status = OrderStatus.Shipped;
    }
    
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order cant be delivered");
        Status = OrderStatus.Delivered;
    }

    private void CalculateTotal(Money orderLinePrice)
    {
       Total = Total.Add(orderLinePrice);
    }
}