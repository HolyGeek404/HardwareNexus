using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Interfaces;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Persistence.Repositories;

namespace OrderApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString
                ?? throw new InvalidOperationException("Connection string 'OrderDb' is not configured.")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
