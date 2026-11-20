using Microsoft.Extensions.Logging;

namespace OrderApi.Infrastructure.Messaging;

internal sealed class LoggingMessagePublisher(ILogger<LoggingMessagePublisher> logger) : IMessagePublisher
{
    public Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Published {MessageType} {MessageId} with key {Key}: {Content}",
            message.Type, message.MessageId, message.Key, message.Content);
        return Task.CompletedTask;
    }
}
