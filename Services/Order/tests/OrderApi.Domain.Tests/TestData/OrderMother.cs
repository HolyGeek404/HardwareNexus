using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Tests.TestData;

/// <summary>
/// Object Mother: gotowe, poprawne obiekty do testów.
/// Każdy test nadpisuje tylko to, co jest dla niego istotne – reszta ma sensowne wartości domyślne.
/// </summary>
internal static class OrderMother
{
    public static readonly DateTime OrderDate = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

    public static OrderLine Line(
        decimal unitPrice = 100m,
        int quantity = 1,
        Currency currency = Currency.PLN,
        Guid? productId = null) =>
        OrderLine.Create(
            ProductId.Create(productId ?? Guid.NewGuid()),
            Quantity.Create(quantity),
            Money.Create(unitPrice, currency));

    public static Order PendingOrder(params OrderLine[] lines)
    {
        var orderLines = lines.Length == 0 ? new List<OrderLine> { Line() } : lines.ToList();

        return Order.Create(
            OrderId.Create(Guid.NewGuid()),
            CustomerId.Create(Guid.NewGuid()),
            OrderDate,
            orderLines);
    }

    /// <summary>
    /// Doprowadza zamówienie do danego statusu wyłącznie publicznymi metodami agregatu –
    /// tak samo, jak zrobiłby to kod produkcyjny. Nie ustawiamy statusu „na skróty”.
    /// </summary>
    public static Order OrderWithStatus(OrderStatus status)
    {
        var order = PendingOrder();

        switch (status)
        {
            case OrderStatus.Pending:
                break;
            case OrderStatus.Paid:
                order.Pay();
                break;
            case OrderStatus.Shipped:
                order.Pay();
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Pay();
                order.Ship();
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "Status not reachable in tests");
        }

        return order;
    }
}
