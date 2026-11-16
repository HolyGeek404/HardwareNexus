using OrderApi.Application.Abstractions;

namespace OrderApi.Application.Features.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyList<PlaceOrderItem> Items) : ICommand<Guid>;

public sealed record PlaceOrderItem(Guid ProductId, int Quantity);

