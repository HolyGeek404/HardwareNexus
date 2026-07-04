using CartApi.Application.DTO;
using CartApi.Application.Services;
using CartApi.Infrastructure.Services;
using StackExchange.Redis;
using static System.Int32;

namespace CartApi.Infrastructure.Repositories;

public class RedisRepository(IConnectionMultiplexer connection,
    ISerializeService serializeService) : IRedisRepository
{
    private readonly IDatabase _redis = connection.GetDatabase();
    private static string GetCartKey(string userId) => $"cart:user:{userId}";
    private static int CartExpirationMinutes() => TryParse(Environment.GetEnvironmentVariable("CART_EXPIRATION_MINUTES"), out var expirationTime) ? expirationTime : 60;

    public async Task AddCartItem(string userId, ProductDto product)
    {
        var key = GetCartKey(userId);
        var productJson = serializeService.Serialize(product);

        await _redis.HashSetAsync(key, product.Id, productJson);
        await _redis.KeyExpireAsync(key, TimeSpan.FromMinutes(CartExpirationMinutes()));
    }

    public async Task<IReadOnlyList<ProductDto?>> GetCartAsync(string userId)
    {
        var cartItems = await _redis.HashGetAllAsync(GetCartKey(userId));
        return cartItems.Select(item => serializeService.Deserialize<ProductDto>(item.Value!)).ToList();
    }

    public async Task<bool> RemoveItemAsync(string userId, string productId) => await _redis.HashDeleteAsync(GetCartKey(userId), productId);
    public async Task ClearCartAsync(string userId) => await _redis.KeyDeleteAsync(GetCartKey(userId));
}