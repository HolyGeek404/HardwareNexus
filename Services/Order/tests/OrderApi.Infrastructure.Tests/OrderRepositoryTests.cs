using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;
using OrderApi.Infrastructure.Persistence.Repositories;

namespace OrderApi.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class OrderRepositoryTests(SqlServerFixture fixture)
{
    private static readonly DateTime OrderDate = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private static Order NewOrder(params (decimal unitPrice, int quantity)[] lines)
    {
        var orderLines = lines
            .Select(l => OrderLine.Create(
                ProductId.Create(Guid.NewGuid()),
                Quantity.Create(l.quantity),
                Money.Create(l.unitPrice, Currency.PLN)))
            .ToList();

        return Order.Create(OrderId.Create(Guid.NewGuid()), 
            CustomerId.Create(Guid.NewGuid()), OrderDate, orderLines).Value;
    }

    private async Task SaveAsync(Order order)
    {
        await using var db = fixture.CreateDbContext();
        await new OrderRepository(db).Add(order, CancellationToken.None);
        await ((IUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
    }

    private async Task<Order?> LoadAsync(OrderId id)
    {
        await using var db = fixture.CreateDbContext();
        return await new OrderRepository(db).GetByIdAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task SaveAndLoad_ReturnsWholeAggregateWithLines()
    {
        var order = NewOrder((100m, 2), (49.99m, 1));

        await SaveAsync(order);
        var loaded = await LoadAsync(order.Id);

        Assert.NotNull(loaded);
        Assert.Equal(order.Id, loaded.Id);
        Assert.Equal(order.CustomerId, loaded.CustomerId);
        Assert.Equal(OrderStatus.Pending, loaded.Status);
        Assert.Equal(OrderDate, loaded.OrderDate);
        Assert.Equal(Money.Create(249.99m, Currency.PLN), loaded.Total);

        Assert.Equal(2, loaded.Lines.Count);
        var line = loaded.Lines.Single(l => l.ProductId == order.Lines.First().ProductId);
        Assert.Equal(Quantity.Create(2), line.Quantity);
        Assert.Equal(Money.Create(100m, Currency.PLN), line.Price);
        Assert.Equal(Money.Create(200m, Currency.PLN), line.TotalPrice);
    }

    [Fact]
    public async Task ChangedStatus_IsPersisted()
    {
        var order = NewOrder((10m, 1));
        await SaveAsync(order);

        await using (var db = fixture.CreateDbContext())
        {
            var repository = new OrderRepository(db);
            var loaded = (await repository.GetByIdAsync(order.Id, CancellationToken.None))!;

            Assert.True(loaded.Pay().IsSuccess);
            
            await ((IUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
        }

        var reloaded = await LoadAsync(order.Id);
        Assert.Equal(OrderStatus.Paid, reloaded!.Status);
    }

    [Fact]
    public async Task AddedLine_IsPersistedAndTotalUpdated()
    {
        var order = NewOrder((10m, 1));
        await SaveAsync(order);

        await using (var db = fixture.CreateDbContext())
        {
            var loaded = (await new OrderRepository(db).GetByIdAsync(order.Id, CancellationToken.None))!;
            var newLine = OrderLine.Create(ProductId.Create(Guid.NewGuid()), Quantity.Create(3), Money.Create(5m, Currency.PLN));

            Assert.True(loaded.AddOrderLine(newLine).IsSuccess);
            await ((IUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
        }

        var reloaded = await LoadAsync(order.Id);
        Assert.Equal(2, reloaded!.Lines.Count);
        Assert.Equal(Money.Create(25m, Currency.PLN), reloaded.Total);
    }

    [Fact]
    public async Task Load_NonExistingOrder_ReturnsNull()
    {
        Assert.Null(await LoadAsync(OrderId.Create(Guid.NewGuid())));
    }
}
