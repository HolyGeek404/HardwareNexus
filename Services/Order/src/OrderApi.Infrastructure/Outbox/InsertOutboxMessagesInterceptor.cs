using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderApi.Application.IntegrationEvents;
using OrderApi.Domain.Primitives;

namespace OrderApi.Infrastructure.Outbox;

internal sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            AddOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            AddOutboxMessages(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static void AddOutboxMessages(DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count != 0)
            .ToList();

        var messages = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(IntegrationEventMapper.FromDomainEvent)
            .OfType<IIntegrationEvent>()
            .Select(ToOutboxMessage)
            .ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        context.Set<OutboxMessage>().AddRange(messages);
    }

    private static OutboxMessage ToOutboxMessage(IIntegrationEvent integrationEvent) => new()
    {
        Id = integrationEvent.EventId,
        Type = integrationEvent.GetType().Name,
        Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
        OccurredOnUtc = integrationEvent.OccurredOnUtc
    };
}
