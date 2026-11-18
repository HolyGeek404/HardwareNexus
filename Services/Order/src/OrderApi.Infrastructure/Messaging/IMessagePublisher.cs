namespace OrderApi.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(Guid messageId, string type, string content, CancellationToken cancellationToken);
}
