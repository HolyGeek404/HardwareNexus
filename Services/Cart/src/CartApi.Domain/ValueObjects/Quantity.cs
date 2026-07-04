namespace CartApi.Domain.ValueObjects;

public record Quantity
{
    public int Value { get; init; }
    private Quantity(int value) => Value = value;

    public static Quantity Create(int value)
    {
        return value > 0 ? new Quantity(value) : throw new ArgumentNullException(nameof(value));
    }
}