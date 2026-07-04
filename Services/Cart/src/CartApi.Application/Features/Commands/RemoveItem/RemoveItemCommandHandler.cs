using CartApi.Application.Services;
using MediatR;

namespace CartApi.Application.Features.Commands.RemoveItem;

public class RemoveItemCommandHandler(ICacheService cacheService) : IRequestHandler<RemoveItemCommand, Unit>
{
    public async Task<Unit> Handle(RemoveItemCommand request, CancellationToken cancellationToken)
    {
        await cacheService.RemoveItemAsync(request.UserId, request.ProductId);
        return Unit.Value;
    }
}
