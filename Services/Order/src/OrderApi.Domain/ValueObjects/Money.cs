namespace OrderApi.Domain.ValueObjects;

public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; } 
    public Currency Currency { get; private set; }

    public static Money Create(decimal amount, Currency currency)
    {
        return amount < 0 ? 
            throw new InvalidOperationException("Invalid amount"): 
            new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
        {
            throw new InvalidOperationException("Currency missmatch");
        }
        return this with{Amount = Amount+other.Amount};
    }
}