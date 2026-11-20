using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderApi.Infrastructure.Messaging;
using OrderApi.Infrastructure.Persistence;

namespace OrderApi.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    OrderDbContext db,
    IMessagePublisher publisher,
    TimeProvider timeProvider,
    OutboxOptions options,
    ILogger<OutboxProcessor> logger)
{
    public async Task<int> ProcessAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var messages = await db.Set<OutboxMessage>()
            .FromSql($"""
                SELECT TOP ({options.BatchSize}) m.*
                FROM OutboxMessages m WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE m.ProcessedOnUtc IS NULL
                  AND m.AttemptCount < {options.MaxAttempts}
                  AND NOT EXISTS (
                      SELECT 1 FROM OutboxMessages dead
                      WHERE dead.[Key] = m.[Key]
                        AND dead.ProcessedOnUtc IS NULL
                        AND dead.AttemptCount >= {options.MaxAttempts})
                ORDER BY m.OccurredOnUtc
                """)
            .ToListAsync(cancellationToken);

        var blockedKeys = new HashSet<string>();

        foreach (var message in messages)
        {
            if (blockedKeys.Contains(message.Key))
                continue;

            try
            {
                await publisher.PublishAsync(
                    new OutgoingMessage(message.Id, message.Type, message.Key, message.Content),
                    cancellationToken);
                message.ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
                message.Error = null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.AttemptCount++;
                message.Error = exception.Message;
                blockedKeys.Add(message.Key);

                logger.LogWarning(exception,
                    "Publishing outbox message {MessageId} ({MessageType}) failed, attempt {Attempt}/{MaxAttempts}",
                    message.Id, message.Type, message.AttemptCount, options.MaxAttempts);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages.Count;
    }
}
