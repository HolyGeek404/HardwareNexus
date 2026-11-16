using MediatR;
using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Features.Commands;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyList<PlaceOrderItem> Items) : IRequest<Result<Guid>>;

public sealed record PlaceOrderItem(Guid ProductId, int Quantity);

