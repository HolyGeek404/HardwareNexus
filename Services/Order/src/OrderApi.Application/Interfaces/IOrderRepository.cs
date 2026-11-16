using OrderApi.Domain.Entities;
using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Application.Interfaces;

public interface IOrderRepository
{
    Task Add(Order order,  CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken);
}