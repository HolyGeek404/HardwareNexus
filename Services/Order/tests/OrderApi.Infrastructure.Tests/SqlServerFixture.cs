using Microsoft.EntityFrameworkCore;
using OrderApi.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace OrderApi.Infrastructure.Tests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public OrderDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
