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
                SELECT TOP ({options.BatchSize}) *
                FROM OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE ProcessedOnUtc IS NULL AND AttemptCount < {options.MaxAttempts}
                ORDER BY OccurredOnUtc
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.Id, message.Type, message.Content, cancellationToken);
                message.ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
                message.Error = null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.AttemptCount++;
                message.Error = exception.Message;

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
