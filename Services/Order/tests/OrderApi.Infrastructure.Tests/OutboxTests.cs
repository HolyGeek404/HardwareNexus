using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Application.IntegrationEvents;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;
using OrderApi.Infrastructure.Outbox;
using OrderApi.Infrastructure.Persistence.Repositories;

namespace OrderApi.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class OutboxTests(SqlServerFixture fixture)
{
    private static readonly DateTime OrderDate = new(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc);

    private static Order NewOrder(OrderId id) =>
        Order.Create(
            id,
            CustomerId.Create(Guid.NewGuid()),
            OrderDate,
            [OrderLine.Create(ProductId.Create(Guid.NewGuid()), Quantity.Create(2), Money.Create(50m, Currency.PLN))]).Value;

    private async Task SaveAsync(Order order)
    {
        await using var db = fixture.CreateDbContext();
        await new OrderRepository(db).Add(order, CancellationToken.None);
        await ((IUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
    }

    private async Task<List<OrderPlacedIntegrationEvent>> OutboxEventsForAsync(OrderId orderId)
    {
        await using var db = fixture.CreateDbContext();

        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.Type == nameof(OrderPlacedIntegrationEvent))
            .ToListAsync();

        return messages
            .Select(m => JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(m.Content)!)
            .Where(e => e.OrderId == orderId.Value)
            .ToList();
    }

    [Fact]
    public async Task SavingNewOrder_WritesIntegrationEventToOutbox()
    {
        var order = NewOrder(OrderId.Create(Guid.NewGuid()));

        await SaveAsync(order);

        var placed = Assert.Single(await OutboxEventsForAsync(order.Id));
        Assert.Equal(order.CustomerId.Value, placed.CustomerId);
        Assert.Equal(100m, placed.TotalAmount);
        Assert.Equal("PLN", placed.Currency);
        Assert.Equal(OrderDate, placed.OccurredOnUtc);
    }

    [Fact]
    public async Task OutboxMessage_IsUnprocessedAfterSave()
    {
        var order = NewOrder(OrderId.Create(Guid.NewGuid()));

        await SaveAsync(order);

        await using var db = fixture.CreateDbContext();
        var placed = Assert.Single(await OutboxEventsForAsync(order.Id));
        var message = await db.Set<OutboxMessage>().SingleAsync(m => m.Id == placed.EventId);
        Assert.Null(message.ProcessedOnUtc);
    }

    [Fact]
    public async Task DomainEvents_AreClearedAfterSave()
    {
        var order = NewOrder(OrderId.Create(Guid.NewGuid()));

        await SaveAsync(order);

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public async Task FailedSave_WritesNeitherOrderNorOutboxMessage()
    {
        var id = OrderId.Create(Guid.NewGuid());
        await SaveAsync(NewOrder(id));

        await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(NewOrder(id)));

        Assert.Single(await OutboxEventsForAsync(id));
    }

    [Fact]
    public void Mapper_TranslatesDomainEventToPrimitiveIntegrationEvent()
    {
        var order = NewOrder(OrderId.Create(Guid.NewGuid()));

        var integrationEvent = IntegrationEventMapper.FromDomainEvent(order.DomainEvents.Single());

        var placed = Assert.IsType<OrderPlacedIntegrationEvent>(integrationEvent);
        Assert.Equal(order.Id.Value, placed.OrderId);
        Assert.NotEqual(Guid.Empty, placed.EventId);
    }
}
