using OrderApi.Application.Abstractions;
using OrderApi.Application.Errors;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Primitives;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Application.Features.Commands.PlaceOrder;

public class PlaceOrderCommandHandler(IOrderRepository orderRepository, 
    IProductCatalog productCatalog, 
    IUnitOfWork uow,
    TimeProvider timeProvider) : ICommandHandler<PlaceOrderCommand,Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var lines = new List<OrderLine>();
        foreach (var item in request.Items)
        {
            var productId = ProductId.Create(item.ProductId);
            var quantity = Quantity.Create(item.Quantity);
            var price = await productCatalog.GetUnitPriceAsync(productId, cancellationToken);
            
            if (price is null)
                return ApplicationErrors.Product.NotFound;
            
            var line = OrderLine.Create(productId, quantity, price);
            lines.Add(line);
        }
        
        var order = Order.Create(OrderId.Create(Guid.NewGuid()), CustomerId.Create(request.CustomerId),
            timeProvider.GetUtcNow().UtcDateTime, lines);

        if (order.IsFailure)
        {
            return order.Error;
        }
        
        await orderRepository.Add(order.Value, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        
        return order.Value.Id.Value;
    }
}