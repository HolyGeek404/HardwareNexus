using Moq;
using OrderApi.Application.Errors;
using OrderApi.Application.Features.Commands;
using OrderApi.Application.Features.Commands.PlaceOrder;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Errors;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Application.Tests;


public class PlaceOrderCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IProductCatalog> _catalog = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<TimeProvider> _time = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _time.Setup(t => t.GetUtcNow()).Returns(Now);

        _catalog
            .Setup(c => c.GetUnitPriceAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(100m, Currency.PLN));

        _handler = new PlaceOrderCommandHandler(_orders.Object, _catalog.Object, _unitOfWork.Object, _time.Object);
    }

    private static PlaceOrderCommand Command(params PlaceOrderItem[] items) =>
        new(Guid.NewGuid(), items);

    [Fact]
    public async Task Handle_WithValidItems_AddsOrderSavesAndReturnsItsId()
    {
        Order? added = null;
        _orders
            .Setup(r => r.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => added = order);

        var command = Command(new PlaceOrderItem(Guid.NewGuid(), 2), new PlaceOrderItem(Guid.NewGuid(), 1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(added.Id.Value, result.Value);
        Assert.Equal(command.CustomerId, added.CustomerId.Value);
        Assert.Equal(2, added.Lines.Count);
        Assert.Equal(Money.Create(300m, Currency.PLN), added.Total);
        Assert.Equal(Now.UtcDateTime, added.OrderDate);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AddsOrderBeforeSavingChanges()
    {
        var calls = new List<string>();
        _orders
            .Setup(r => r.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Add"));
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Save"));

        await _handler.Handle(Command(new PlaceOrderItem(Guid.NewGuid(), 1)), CancellationToken.None);

        Assert.Equal(["Add", "Save"], calls);
    }

    [Fact]
    public async Task Handle_WhenProductNotInCatalog_ReturnsNotFoundAndSavesNothing()
    {
        var missingProduct = Guid.NewGuid();
        _catalog
            .Setup(c => c.GetUnitPriceAsync(ProductId.Create(missingProduct), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

        var result = await _handler.Handle(
            Command(new PlaceOrderItem(Guid.NewGuid(), 1), new PlaceOrderItem(missingProduct, 1)),
            CancellationToken.None);

        Assert.Equal(ApplicationErrors.Product.NotFound, result.Error);
        AssertNothingPersisted();
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsOrder_ReturnsDomainErrorAndSavesNothing()
    {
        // Handler nie sprawdza duplikatów sam – przekazuje błąd z agregatu.
        var sameProduct = Guid.NewGuid();

        var result = await _handler.Handle(
            Command(new PlaceOrderItem(sameProduct, 1), new PlaceOrderItem(sameProduct, 2)),
            CancellationToken.None);

        Assert.Equal(DomainErrors.Order.DuplicateProduct, result.Error);
        AssertNothingPersisted();
    }

    [Fact]
    public async Task Handle_WithNoItems_ReturnsEmptyOrderErrorAndSavesNothing()
    {
        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(DomainErrors.Order.Empty, result.Error);
        AssertNothingPersisted();
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToPorts()
    {
        using var cts = new CancellationTokenSource();

        await _handler.Handle(Command(new PlaceOrderItem(Guid.NewGuid(), 1)), cts.Token);

        _catalog.Verify(c => c.GetUnitPriceAsync(It.IsAny<ProductId>(), cts.Token), Times.Once);
        _orders.Verify(r => r.Add(It.IsAny<Order>(), cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    private void AssertNothingPersisted()
    {
        _orders.Verify(r => r.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
