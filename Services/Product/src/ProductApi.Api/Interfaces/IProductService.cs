using System.Text.Json;
using ProductApi.Api.Models;

namespace ProductApi.Api.Interfaces;

public interface IProductService
{
    Task<object?> GetByIdAndType(ProductCategory type, string id);
    Task<object?> GetByType(ProductCategory type);
    Task<object?> GetFilterByType(ProductCategory type);
    Task Create(JsonElement product, ProductCategory type);
    Task Delete(Guid id, ProductCategory type);
    Task Update(JsonElement product, ProductCategory type);
}