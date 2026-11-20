using System.Text;
using Confluent.Kafka;
using OrderApi.Infrastructure.Messaging;

namespace OrderApi.Infrastructure.Tests;

public class KafkaMessagePublisherTests(KafkaFixture kafka) : IClassFixture<KafkaFixture>
{
    private List<ConsumeResult<string, string>> ConsumeAll(string topic, int expectedCount)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafka.BootstrapServers,
            GroupId = $"test-group-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        var results = new List<ConsumeResult<string, string>>();
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (results.Count < expectedCount && DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(1));
            if (result is not null)
                results.Add(result);
        }

        consumer.Close();
        return results;
    }

    private static string Header(ConsumeResult<string, string> result, string name) =>
        Encoding.UTF8.GetString(result.Message.Headers.GetLastBytes(name));

    [Fact]
    public async Task Publish_SendsKeyContentAndHeaders()
    {
        var topic = await kafka.CreateTopicAsync(partitions: 1);
        var message = new OutgoingMessage(Guid.NewGuid(), "OrderPlacedIntegrationEvent", Guid.NewGuid().ToString(), """{"total":100}""");

        using (var publisher = new KafkaMessagePublisher(new KafkaOptions { BootstrapServers = kafka.BootstrapServers, Topic = topic }))
            await publisher.PublishAsync(message, CancellationToken.None);

        var received = Assert.Single(ConsumeAll(topic, expectedCount: 1));
        Assert.Equal(message.Key, received.Message.Key);
        Assert.Equal(message.Content, received.Message.Value);
        Assert.Equal(message.MessageId.ToString(), Header(received, KafkaMessagePublisher.MessageIdHeader));
        Assert.Equal(message.Type, Header(received, KafkaMessagePublisher.MessageTypeHeader));
    }

    [Fact]
    public async Task MessagesWithSameKey_LandInSamePartition_InPublishOrder()
    {
        var topic = await kafka.CreateTopicAsync(partitions: 3);
        var orderId = Guid.NewGuid().ToString();
        var events = new[] { "Placed", "Paid", "Shipped" };

        using (var publisher = new KafkaMessagePublisher(new KafkaOptions { BootstrapServers = kafka.BootstrapServers, Topic = topic }))
        {
            foreach (var type in events)
                await publisher.PublishAsync(new OutgoingMessage(Guid.NewGuid(), type, orderId, "{}"), CancellationToken.None);
        }

        var received = ConsumeAll(topic, expectedCount: events.Length);

        Assert.Single(received.Select(r => r.Partition.Value).Distinct());
        Assert.Equal(events, received.OrderBy(r => r.Offset.Value).Select(r => Header(r, KafkaMessagePublisher.MessageTypeHeader)));
    }
}
