using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;

namespace OrderApi.Infrastructure.Tests;

public sealed class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _container =
        new KafkaBuilder("confluentinc/cp-kafka:7.6.1").Build();

    public string BootstrapServers => _container.GetBootstrapAddress();

    public async Task<string> CreateTopicAsync(int partitions)
    {
        var topic = $"test-{Guid.NewGuid():N}";

        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build();
        await admin.CreateTopicsAsync([new TopicSpecification { Name = topic, NumPartitions = partitions, ReplicationFactor = 1 }]);

        return topic;
    }

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
