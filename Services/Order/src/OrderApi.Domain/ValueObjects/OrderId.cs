namespace OrderApi.Domain.ValueObjects;

public record OrderId
{
    public Guid Value { get; private set; }

    private OrderId(Guid value)
    {
        Value = value;
    }

    public static OrderId Create(Guid value)
    {
        return value == Guid.Empty ? 
            throw new ArgumentException("Id cannot be empty"):
            new OrderId(value);
    }
}