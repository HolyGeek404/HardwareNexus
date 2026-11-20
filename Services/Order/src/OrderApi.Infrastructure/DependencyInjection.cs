using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderApi.Application.Interfaces;
using OrderApi.Infrastructure.Messaging;
using OrderApi.Infrastructure.Outbox;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Persistence.Repositories;

namespace OrderApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderDb");

        services.AddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
            options
                .UseSqlServer(connectionString
                    ?? throw new InvalidOperationException("Connection string 'OrderDb' is not configured."))
                .AddInterceptors(serviceProvider.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(new OutboxOptions());
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();

        AddMessaging(services, configuration);

        return services;
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var kafka = configuration.GetSection(KafkaOptions.SectionName);
        var bootstrapServers = kafka[nameof(KafkaOptions.BootstrapServers)];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            services.AddSingleton<IMessagePublisher, LoggingMessagePublisher>();
            return;
        }

        services.AddSingleton(new KafkaOptions
        {
            BootstrapServers = bootstrapServers,
            Topic = kafka[nameof(KafkaOptions.Topic)] ?? new KafkaOptions().Topic
        });
        services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
    }
}
