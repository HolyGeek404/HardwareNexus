using System.Text.Json;
using ProductApi.Api.Factories;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Services;

public class ProductService(IProductRepositoryFactory factory) : IProductService
{
    #region Queries

    public async Task<object?> GetByIdAndType(ProductCategory type, string id)
    {
        var repo = factory.Create(type);
        return await repo.GetById(id);
    }

    public async Task<object?> GetByType(ProductCategory type)
    {
        var repo = factory.Create(type);
        return await repo.GetByType();
    }

    public async Task<object?> GetFilterByType(ProductCategory type)
    {
        var repo = factory.Create(type);
        return await repo.GetFiltersAsync();
    }

    #endregion

    #region Commands

    public async Task Create(JsonElement product, ProductCategory type)
    {
        var repo = factory.Create(type);
        await repo.CreateAsync(product);
    }

    public async Task Delete(Guid id, ProductCategory type)
    {
        var repo = factory.Create(type);
        await repo.DeleteAsync(id);
    }

    public async Task Update(JsonElement product, ProductCategory type)
    {
        var repo = factory.Create(type);
        await repo.UpdateAsync(product);
    }

    #endregion
}