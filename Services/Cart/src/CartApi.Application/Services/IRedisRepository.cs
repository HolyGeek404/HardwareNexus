using CartApi.Application.DTO;

namespace CartApi.Application.Services;

public interface IRedisRepository
{
    Task AddCartItem(string userId, ProductDto product);
    Task<IReadOnlyList<ProductDto?>> GetCartAsync(string userId);
    Task<bool> RemoveItemAsync(string userId, string productId);
    Task ClearCartAsync(string userId);
}