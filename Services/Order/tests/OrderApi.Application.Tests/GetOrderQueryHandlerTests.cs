using Moq;
using OrderApi.Application.DTO;
using OrderApi.Application.Errors;
using OrderApi.Application.Features.Queries.GetOrder;
using OrderApi.Application.Interfaces;

namespace OrderApi.Application.Tests;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderQueries> _queries = new();
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _handler = new GetOrderQueryHandler(_queries.Object);
    }

    private static OrderDto SampleOrder(Guid orderId) => new()
    {
        OrderId = orderId,
        CustomerId = Guid.NewGuid(),
        Total = 250m,
        Currency = "PLN",
        Status = "Pending",
        OrderDate = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc),
        OrderLines =
        [
            new OrderLineDto { ProductId = Guid.NewGuid(), UnitPrice = 100m, Quantity = 2, TotalPrice = 200m },
            new OrderLineDto { ProductId = Guid.NewGuid(), UnitPrice = 50m, Quantity = 1, TotalPrice = 50m }
        ]
    };

    [Fact]
    public async Task Handle_WhenOrderExists_ReturnsItsDto()
    {
        var orderId = Guid.NewGuid();
        var dto = SampleOrder(orderId);
        _queries
            .Setup(q => q.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto, result.Value);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsOrderNotFound()
    {
        _queries
            .Setup(q => q.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        var result = await _handler.Handle(new GetOrderQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Order.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_AsksPortForRequestedIdWithCancellationToken()
    {
        var orderId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        await _handler.Handle(new GetOrderQuery(orderId), cts.Token);

        _queries.Verify(q => q.GetByIdAsync(orderId, cts.Token), Times.Once);
    }
}
