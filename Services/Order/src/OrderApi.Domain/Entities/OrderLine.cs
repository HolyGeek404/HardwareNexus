using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class OrderLine
{
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money Price { get; private set; }
    public Money TotalPrice { get; private set; }

    private OrderLine(ProductId productId, Quantity quantity, Money price)
    {
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        TotalPrice = Money.Create(price.Amount*quantity.Amount,  price.Currency);
    }

    public static OrderLine Create(ProductId productId, Quantity quantity, Money price)
    {
       return new OrderLine(productId, quantity, price);
    }
}