using Microsoft.EntityFrameworkCore;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderDbContext db) : IOrderRepository
{
    public Task Add(Order order, CancellationToken cancellationToken)
    {
        db.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken) =>
        db.Orders.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
}
