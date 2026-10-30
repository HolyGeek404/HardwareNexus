using System.Text.Json;
using Moq;
using ProductApi.Api.Factories;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;
using ProductApi.Api.Services;

namespace ProductApi.Api.Tests.Unit;

public class ProductServiceTests
{
    private const ProductCategory UnsupportedCategory = (ProductCategory)999;

    private readonly Mock<IProductRepositoryFactory> _factory = new();
    private readonly Mock<IProductRepository> _repository = new();
    private readonly ProductService _service;
    private readonly CancellationToken _ct = new CancellationTokenSource().Token;

    public ProductServiceTests()
    {
        _factory.Setup(f => f.Create(It.IsAny<ProductCategory>())).Returns(_repository.Object);
        _service = new ProductService(_factory.Object);
    }

    #region Queries

    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task GetByIdAndType_ResolvesRepositoryForCategory_AndReturnsProduct(ProductCategory type)
    {
        var product = new object();
        _repository.Setup(r => r.GetById("abc", _ct)).ReturnsAsync(product);

        var result = await _service.GetByIdAndType(type, "abc");

        Assert.Same(product, result);
        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.GetById("abc", _ct), Times.Once);
    }

    [Fact]
    public async Task GetByIdAndType_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetById("missing", _ct)).ReturnsAsync((object?)null);

        var result = await _service.GetByIdAndType(ProductCategory.Cpu, "missing");

        Assert.Null(result);
    }

    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task GetByType_ResolvesRepositoryForCategory_AndReturnsProducts(ProductCategory type)
    {
        IReadOnlyList<object> products = [new object(), new object()];
        _repository.Setup(r => r.GetByType(_ct)).ReturnsAsync(products);

        var result = await _service.GetByType(type);

        Assert.Same(products, result);
        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.GetByType(_ct), Times.Once);
    }

    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task GetFilterByType_ResolvesRepositoryForCategory_AndReturnsFilters(ProductCategory type)
    {
        var filters = new object();
        _repository.Setup(r => r.GetFiltersAsync(_ct)).ReturnsAsync(filters);

        var result = await _service.GetFilterByType(type);

        Assert.Same(filters, result);
        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.GetFiltersAsync(_ct), Times.Once);
    }

    #endregion

    #region Commands

    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task Create_ForwardsPayloadToRepositoryForCategory(ProductCategory type)
    {
        var payload = JsonSerializer.SerializeToElement(new { name = "Test" });

        await _service.Create(payload, type);

        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.CreateAsync(payload, _ct), Times.Once);
    }

    [Fact]
    public async Task Create_WhenRepositoryRejectsPayload_PropagatesJsonException()
    {
        var payload = JsonSerializer.SerializeToElement(new { name = "Test" });
        _repository.Setup(r => r.CreateAsync(payload, _ct)).ThrowsAsync(new JsonException("invalid"));

        await Assert.ThrowsAsync<JsonException>(() => _service.Create(payload, ProductCategory.Cpu));
    }

    [Theory]
    [InlineData(ProductCategory.Cpu, true)]
    [InlineData(ProductCategory.Cpu, false)]
    [InlineData(ProductCategory.Gpu, true)]
    [InlineData(ProductCategory.Gpu, false)]
    [InlineData(ProductCategory.Cooler, true)]
    [InlineData(ProductCategory.Cooler, false)]
    public async Task Update_ReturnsRepositoryResult(ProductCategory type, bool repositoryResult)
    {
        var payload = JsonSerializer.SerializeToElement(new { name = "Test" });
        _repository.Setup(r => r.UpdateAsync(payload, _ct)).ReturnsAsync(repositoryResult);

        await _service.Update(payload, type);

        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.UpdateAsync(payload, _ct), Times.Once);
    }

    [Theory]
    [InlineData(ProductCategory.Cpu, true)]
    [InlineData(ProductCategory.Cpu, false)]
    [InlineData(ProductCategory.Gpu, true)]
    [InlineData(ProductCategory.Gpu, false)]
    [InlineData(ProductCategory.Cooler, true)]
    [InlineData(ProductCategory.Cooler, false)]
    public async Task Delete_ReturnsRepositoryResult(ProductCategory type, bool repositoryResult)
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.DeleteAsync(id, _ct)).ReturnsAsync(repositoryResult);

        await _service.Delete(id, type);

        _factory.Verify(f => f.Create(type), Times.Once);
        _repository.Verify(r => r.DeleteAsync(id, _ct), Times.Once);
    }

    #endregion
}