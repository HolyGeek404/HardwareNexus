using CartApi.Application.DTO;
using CartApi.Application.Services;
using CartApi.Domain;

namespace CartApi.Infrastructure.Services;

public class CacheService(IRedisRepository redisRepository) : ICacheService
{
    public async Task AddItemAsync(string userId, Product product)
    {
        await redisRepository.AddCartItem(userId, ProductMapper.Map(product));
    }

    public async Task<IReadOnlyList<ProductDto>> GetCartAsync(string userId)
    {
        var products = await redisRepository.GetCartAsync(userId);
        return products.Where(product => product is not null).Select(product => product!).ToList();
    }

    public async Task<bool> RemoveItemAsync(string userId, string productId)
    {
        return await redisRepository.RemoveItemAsync(userId, productId);
    }

    public async Task ClearCartAsync(string userId)
    {
        await redisRepository.ClearCartAsync(userId);
    }
}