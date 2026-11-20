namespace OrderApi.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken);
}

public sealed record OutgoingMessage(Guid MessageId, string Type, string Key, string Content);
