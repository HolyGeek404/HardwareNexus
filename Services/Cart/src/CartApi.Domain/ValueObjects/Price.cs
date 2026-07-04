namespace CartApi.Domain.ValueObjects;

public record Price
{
    public int Value { get; init; }
    private Price(int value) => Value = value;

    public static Price Create(int value)
    {
        return value >= 0 ? new Price(value) : throw new ArgumentNullException(nameof(value));
    }
}