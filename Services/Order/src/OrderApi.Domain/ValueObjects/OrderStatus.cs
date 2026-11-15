namespace OrderApi.Domain.ValueObjects;

public enum OrderStatus
{
    Unknown,
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled,
}