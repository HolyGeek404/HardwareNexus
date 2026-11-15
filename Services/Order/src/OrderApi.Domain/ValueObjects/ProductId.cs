namespace OrderApi.Domain.ValueObjects;

public record ProductId
{
    public Guid Value { get; private set; }

    private ProductId(Guid value)
    {
        Value = value;
    }

    public static ProductId Create(Guid value)
    {
        return value == Guid.Empty ? 
            throw new ArgumentException("Id cannot be empty"):
            new ProductId(value);
    }
}