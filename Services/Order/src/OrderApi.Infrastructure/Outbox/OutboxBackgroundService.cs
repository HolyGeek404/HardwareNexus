using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderApi.Infrastructure.Outbox;

internal sealed class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    OutboxOptions options,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.PollingInterval);

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await processor.ProcessAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Outbox processing cycle failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
