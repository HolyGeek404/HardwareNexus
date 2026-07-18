using Azure.Identity;
using CartApi.Application.Services;
using CartApi.Infrastructure.Repositories;
using CartApi.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CartApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnStr = Environment.GetEnvironmentVariable("REDIS_CONNSTR");
            if (redisConnStr != null) return ConnectionMultiplexer.Connect(redisConnStr!);

            configuration.AddAzureKeyVault(new Uri(configuration.GetSection("AzureAd")["KvUrl"]!),
                new DefaultAzureCredential());
            redisConnStr = configuration.GetSection("Redis")["LocalConnStr"];

            return ConnectionMultiplexer.Connect(redisConnStr!);
        });

        services.AddScoped<ISerializeService, SerializeService>();
        services.AddScoped<IRedisRepository, RedisRepository>();

        return services;
    }
}