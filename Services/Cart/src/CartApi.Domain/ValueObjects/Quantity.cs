namespace CartApi.Domain.ValueObjects;

public record Quantity
{
    private Quantity(int value)
    {
        Value = value;
    }

    public int Value { get; init; }

    public static Quantity Create(int value)
    {
        return value > 0 ? new Quantity(value) : throw new ArgumentNullException(nameof(value));
    }
}