using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class OrderLine  : Entity<ProductId>
{
    public ProductId ProductId => Id;
    public Quantity Quantity { get; private set; }
    public Money Price { get; private set; }
    public Money TotalPrice { get; private set; }

    private OrderLine(ProductId productId, Quantity quantity, Money price) : base(productId)
    {
        Quantity = quantity;
        Price = price;
        TotalPrice = Money.Create(price.Amount*quantity.Amount,  price.Currency);
    }

    public static OrderLine Create(ProductId productId, Quantity quantity, Money price)
    {
       return new OrderLine(productId, quantity, price);
    }
}