namespace OrderApi.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; init; } = 20;
    public int MaxAttempts { get; init; } = 5;
}
