using OrderApi.Application.DTO;

namespace OrderApi.Application.Interfaces;

public interface IOrderQueries
{
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
}