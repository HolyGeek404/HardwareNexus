using Microsoft.Extensions.Logging;

namespace OrderApi.Infrastructure.Messaging;

internal sealed class LoggingMessagePublisher(ILogger<LoggingMessagePublisher> logger) : IMessagePublisher
{
    public Task PublishAsync(Guid messageId, string type, string content, CancellationToken cancellationToken)
    {
        logger.LogInformation("Published {MessageType} {MessageId}: {Content}", type, messageId, content);
        return Task.CompletedTask;
    }
}
