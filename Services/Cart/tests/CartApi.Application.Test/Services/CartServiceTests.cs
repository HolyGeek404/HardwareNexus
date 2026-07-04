
using CartApi.Application.DTO;
using CartApi.Application.Services;
using CartApi.Domain;
using CartApi.Domain.ValueObjects;
using CartApi.Infrastructure.Services;
using Moq;
using Xunit;

namespace CartApi.Application.Test.Services;

public class CacheServiceTests
{
    private readonly Mock<IRedisRepository> _redisRepositoryMock;
    private readonly ICacheService _cacheService;
    private readonly Product _product = new()
    {
        Id = "432",
        Name = "GPU",
        Price = Price.Create(32),
        Quantity = Quantity.Create(3)
    };

    private const string UserId = "test";


    public CacheServiceTests()
    {
        _redisRepositoryMock = new Mock<IRedisRepository>();
        _cacheService = new CacheService(_redisRepositoryMock.Object);
    }

    [Fact]
    public async Task AddItemAsync_AddItem_Success()
    {
        // Arrange
        var expectedDto = new ProductDto
        {
            Id = _product.Id,
            Name = _product.Name,
            Price = _product.Price.Value,
            Quantity = _product.Quantity.Value,
        };

        // Act
        await _cacheService.AddItemAsync(UserId, _product);

        // Assert
        _redisRepositoryMock.Verify(x => x.AddCartItem(
            UserId,
            It.Is<ProductDto>(p =>
                p.Id == expectedDto.Id &&
                p.Name == expectedDto.Name &&
                p.Price == expectedDto.Price &&
                p.Quantity == expectedDto.Quantity)),
            Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_WhenRepositoryReturnsNullItem_FiltersOutNulls()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = "p-1",
            Name = "CPU",
            Price = 1000,
            Quantity = 1
        };

        _redisRepositoryMock
            .Setup(x => x.GetCartAsync(UserId))
            .ReturnsAsync(new List<ProductDto?> { product, null });

        // Act
        var result = await _cacheService.GetCartAsync(UserId);

        // Assert
        Assert.Single(result);
        Assert.Equal("p-1", result.First().Id);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
    {
        // Arrange
        const string userId = "test-user";
        const string productId = "p-1";

        _redisRepositoryMock
            .Setup(x => x.RemoveItemAsync(userId, productId))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.RemoveItemAsync(userId, productId);

        // Assert
        Assert.True(result);
        _redisRepositoryMock.Verify(x => x.RemoveItemAsync(userId, productId), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_CallsRepository()
    {
        // Arrange
        const string userId = "test-user";

        // Act
        await _cacheService.ClearCartAsync(userId);

        // Assert
        _redisRepositoryMock.Verify(x => x.ClearCartAsync(userId), Times.Once);
    }
}
