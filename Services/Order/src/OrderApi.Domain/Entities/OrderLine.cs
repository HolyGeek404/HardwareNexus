using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Entities;

public class OrderLine  : Entity<ProductId>
{
    public ProductId ProductId => Id;
    public Quantity Quantity { get; private set; }
    public Money Price { get; private set; }

    public Money TotalPrice => Money.Create(Price.Amount * Quantity.Amount, Price.Currency);

#pragma warning disable CS8618
    private OrderLine()
    {
    }
#pragma warning restore CS8618

    private OrderLine(ProductId productId, Quantity quantity, Money price) : base(productId)
    {
        Quantity = quantity;
        Price = price;
    }

    public static OrderLine Create(ProductId productId, Quantity quantity, Money price)
    {
       return new OrderLine(productId, quantity, price);
    }
}