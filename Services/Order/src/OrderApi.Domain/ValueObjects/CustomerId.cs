namespace OrderApi.Domain.ValueObjects;

public record CustomerId
{
    public Guid Value { get; private set; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId Create(Guid value)
    {
        return value == Guid.Empty ? 
            throw new ArgumentException("Id cannot be empty"):
            new CustomerId(value);
    }
}