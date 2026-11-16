using OrderApi.Application.Abstractions;
using OrderApi.Application.DTO;
using OrderApi.Application.Errors;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Features.Queries.GetOrder;

public class GetOrderQueryHandler(IOrderQueries orderQueries) : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var result = await orderQueries.GetByIdAsync(request.OrderId,  cancellationToken);
        if (result is null)
        {
            return ApplicationErrors.Order.NotFound;
        }
        
        return result;
    }
}