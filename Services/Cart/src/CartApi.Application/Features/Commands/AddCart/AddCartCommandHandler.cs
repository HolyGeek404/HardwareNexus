using CartApi.Application.Services;
using CartApi.Domain;
using CartApi.Domain.ValueObjects;
using MediatR;

namespace CartApi.Application.Features.Commands.AddCart;

public class AddCartCommandHandler(ICacheService cacheService) : IRequestHandler<AddCartCommand, Unit>
{
    public async Task<Unit> Handle(AddCartCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = request.ProductId,
            Name = request.Name,
            Quantity = Quantity.Create(request.Quantity),
            Price = Price.Create(request.Price)
        };

        await cacheService.AddItemAsync(request.UserId, product);
        return Unit.Value;
    }
}