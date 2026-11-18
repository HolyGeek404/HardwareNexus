using OrderApi.Domain.Events;
using OrderApi.Domain.Primitives;

namespace OrderApi.Application.IntegrationEvents;

public static class IntegrationEventMapper
{
    public static IIntegrationEvent? FromDomainEvent(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            OrderPlacedDomainEvent e => new OrderPlacedIntegrationEvent(
                Guid.NewGuid(),
                e.OrderId.Value,
                e.CustomerId.Value,
                e.Total.Amount,
                e.Total.Currency.ToString(),
                e.OccurredOnUtc),
            _ => null
        };
}
