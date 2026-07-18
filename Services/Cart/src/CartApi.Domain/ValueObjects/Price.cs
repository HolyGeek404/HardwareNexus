namespace CartApi.Domain.ValueObjects;

public record Price
{
    private Price(int value)
    {
        Value = value;
    }

    public int Value { get; init; }

    public static Price Create(int value)
    {
        return value >= 0 ? new Price(value) : throw new ArgumentNullException(nameof(value));
    }
}