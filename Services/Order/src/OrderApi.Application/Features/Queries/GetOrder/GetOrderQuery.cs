using OrderApi.Application.Abstractions;
using OrderApi.Application.DTO;

namespace OrderApi.Application.Features.Queries.GetOrder;

public record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;
