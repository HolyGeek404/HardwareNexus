using CartApi.Application.DTO;
using CartApi.Domain;

namespace CartApi.Application.Services;

public interface ICacheService
{
    Task AddItemAsync(string userId, Product product);
    Task<IReadOnlyList<ProductDto>> GetCartAsync(string userId);
    Task<bool> RemoveItemAsync(string userId, string productId);
    Task ClearCartAsync(string userId);
}