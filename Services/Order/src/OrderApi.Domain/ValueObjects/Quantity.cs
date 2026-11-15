namespace OrderApi.Domain.ValueObjects;

public sealed record Quantity
{
    public int Amount { get; private set; }

    private Quantity(int amount)
    {
        Amount = amount;
    }

    public static Quantity Create(int amount)
    {
        return amount <= 0 ? 
            throw new InvalidOperationException("Cannot create a negative amount."): 
            new Quantity(amount);
    }

    public Quantity AddAmount(int amount)
    {
        return amount <= 0 ? 
            throw new InvalidOperationException("Cannot add a negative or zero amount."):
            new Quantity(Amount + amount);
    }

    public Quantity SubtractAmount(int amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Cannot subtract a negative amount.");
        
        return Amount -  amount <= 0 ? 
            throw new InvalidOperationException("Cannot subtract belowe a negative amount."):
            new Quantity(Amount - amount);
    }
}