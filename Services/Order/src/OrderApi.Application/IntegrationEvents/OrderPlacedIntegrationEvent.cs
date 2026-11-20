namespace OrderApi.Application.IntegrationEvents;

public sealed record OrderPlacedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime OccurredOnUtc) : IIntegrationEvent
{
    public Guid AggregateId => OrderId;
}
