using OrderApi.Domain.ValueObjects;
using static OrderApi.Domain.Tests.TestData.OrderMother;

namespace OrderApi.Domain.Tests;

public class ValueObjectTests
{
    // --- Money ---

    [Fact]
    public void Money_Create_WithNegativeAmount_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Money.Create(-0.01m, Currency.PLN));
    }

    [Fact]
    public void Money_Create_WithZero_IsAllowed()
    {
        var money = Money.Create(0m, Currency.PLN);

        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Money_WithSameAmountAndCurrency_AreEqual()
    {
        // Równość po wartości – cecha każdego value objectu.
        Assert.Equal(Money.Create(10m, Currency.PLN), Money.Create(10m, Currency.PLN));
    }

    [Fact]
    public void Money_Add_SameCurrency_ReturnsSum()
    {
        var sum = Money.Create(10m, Currency.PLN).Add(Money.Create(5m, Currency.PLN));

        Assert.Equal(Money.Create(15m, Currency.PLN), sum);
    }

    [Fact]
    public void Money_Add_DifferentCurrency_Throws()
    {
        var pln = Money.Create(10m, Currency.PLN);

        Assert.Throws<InvalidOperationException>(() => pln.Add(Money.Create(5m, Currency.EUR)));
    }

    [Fact]
    public void Money_Add_DoesNotModifyOriginal()
    {
        // Niezmienność: operacja zwraca nowy obiekt.
        var original = Money.Create(10m, Currency.PLN);

        original.Add(Money.Create(5m, Currency.PLN));

        Assert.Equal(10m, original.Amount);
    }

    // --- Quantity ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantity_Create_WithZeroOrNegative_Throws(int amount)
    {
        Assert.Throws<InvalidOperationException>(() => Quantity.Create(amount));
    }

    [Fact]
    public void Quantity_Subtract_BelowOne_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Quantity.Create(2).SubtractAmount(2));
    }

    [Fact]
    public void Quantity_Add_ReturnsNewQuantity()
    {
        Assert.Equal(Quantity.Create(5), Quantity.Create(2).AddAmount(3));
    }

    // --- Identyfikatory ---

    [Fact]
    public void OrderId_Create_WithEmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => OrderId.Create(Guid.Empty));
    }

    [Fact]
    public void CustomerId_Create_WithEmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => CustomerId.Create(Guid.Empty));
    }

    [Fact]
    public void ProductId_Create_WithEmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => ProductId.Create(Guid.Empty));
    }

    // --- OrderLine ---

    [Fact]
    public void OrderLine_TotalPrice_IsUnitPriceTimesQuantity()
    {
        var line = Line(unitPrice: 25m, quantity: 4);

        Assert.Equal(Money.Create(100m, Currency.PLN), line.TotalPrice);
    }
}
