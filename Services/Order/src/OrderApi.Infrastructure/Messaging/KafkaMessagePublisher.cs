using System.Text;
using Confluent.Kafka;

namespace OrderApi.Infrastructure.Messaging;

internal sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    public const string MessageIdHeader = "message-id";
    public const string MessageTypeHeader = "message-type";

    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaMessagePublisher(KafkaOptions options)
    {
        _topic = options.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = 30_000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = message.Key,
            Value = message.Content,
            Headers = new Headers
            {
                { MessageIdHeader, Encoding.UTF8.GetBytes(message.MessageId.ToString()) },
                { MessageTypeHeader, Encoding.UTF8.GetBytes(message.Type) }
            }
        };

        await _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
