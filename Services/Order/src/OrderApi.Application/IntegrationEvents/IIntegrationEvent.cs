namespace OrderApi.Application.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    Guid AggregateId { get; }
    DateTime OccurredOnUtc { get; }
}