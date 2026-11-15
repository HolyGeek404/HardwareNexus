using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class Order
{
    public OrderId Id  { get; private set; }
    public DateTime OrderDate { get; private set; }
    public CustomerId CustomerId { get; private set; }
    private List<OrderLine> OrderLines { get; } = [];
    public IReadOnlyCollection<OrderLine> Lines => OrderLines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

    private Order(OrderId orderId ,CustomerId customer, DateTime orderDate)
    {
        Id = orderId;
        CustomerId = customer;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
    }
    public static Order Create(OrderId orderId, CustomerId customer, DateTime orderDate, List<OrderLine> orderLines)
    {
        if (orderLines.Count == 0)
            throw new InvalidOperationException("Order cant be empty");
        
        if(orderLines.GroupBy(x => x.Price.Currency).Count() > 1)
            throw new InvalidOperationException("Multiple currencies are not allowed");
        
        var order = new Order(orderId, customer, orderDate)
        {
            Total = Money.Create(0, orderLines[0].Price.Currency)
        };

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
            
            if(OrderLines.Any(x => x.ProductId == orderLine.ProductId))
                throw new InvalidOperationException("Duplicated order line");
        }
        OrderLines.Add(orderLine);
        CalculateTotal(Money.Create(orderLine.TotalPrice.Amount, orderLine.Price.Currency));
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