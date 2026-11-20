using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderApi.Infrastructure.Messaging;
using OrderApi.Infrastructure.Outbox;
using OrderApi.Infrastructure.Persistence;

namespace OrderApi.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class OutboxProcessorTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private static readonly OutboxOptions Options = new() { BatchSize = 50, MaxAttempts = 3 };

    private sealed class RecordingPublisher(
        TimeSpan delay = default,
        bool fail = false,
        Func<OutgoingMessage, bool>? failWhen = null) : IMessagePublisher
    {
        public ConcurrentBag<Guid> Published { get; } = [];

        public async Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            if (fail || failWhen?.Invoke(message) == true)
                throw new InvalidOperationException("Broker unavailable");

            Published.Add(message.MessageId);
        }
    }

    private static OutboxProcessor Processor(OrderDbContext db, IMessagePublisher publisher) =>
        new(db, publisher, TimeProvider.System, Options, NullLogger<OutboxProcessor>.Instance);

    public async Task InitializeAsync()
    {
        await using var db = fixture.CreateDbContext();
        await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProcessedOnUtc, DateTime.UtcNow));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private Task<List<Guid>> SeedMessagesAsync(int count) =>
        SeedMessagesAsync(Enumerable.Range(0, count).Select(_ => Guid.NewGuid().ToString()).ToArray());

    private async Task<List<Guid>> SeedMessagesAsync(string[] keys, int attemptCountOfFirst = 0)
    {
        var messages = keys
            .Select((key, i) => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "TestEvent",
                Key = key,
                Content = "{}",
                OccurredOnUtc = DateTime.UtcNow.AddSeconds(i),
                AttemptCount = i == 0 ? attemptCountOfFirst : 0
            })
            .ToList();

        await using var db = fixture.CreateDbContext();
        db.Set<OutboxMessage>().AddRange(messages);
        await db.SaveChangesAsync();

        return messages.Select(m => m.Id).ToList();
    }

    private async Task<OutboxMessage> LoadAsync(Guid id)
    {
        await using var db = fixture.CreateDbContext();
        return await db.Set<OutboxMessage>().SingleAsync(m => m.Id == id);
    }

    [Fact]
    public async Task Process_PublishesUnprocessedMessages_AndMarksThemProcessed()
    {
        var ids = await SeedMessagesAsync(3);
        var publisher = new RecordingPublisher();

        await using (var db = fixture.CreateDbContext())
            await Processor(db, publisher).ProcessAsync(CancellationToken.None);

        Assert.Equal(ids.Order(), publisher.Published.Order());
        foreach (var id in ids)
            Assert.NotNull((await LoadAsync(id)).ProcessedOnUtc);
    }

    [Fact]
    public async Task Process_DoesNotPublishAlreadyProcessedMessagesAgain()
    {
        await SeedMessagesAsync(2);

        await using (var db = fixture.CreateDbContext())
            await Processor(db, new RecordingPublisher()).ProcessAsync(CancellationToken.None);

        var secondRun = new RecordingPublisher();
        await using (var db = fixture.CreateDbContext())
            await Processor(db, secondRun).ProcessAsync(CancellationToken.None);

        Assert.Empty(secondRun.Published);
    }

    [Fact]
    public async Task Process_WhenPublishFails_KeepsMessageForRetryAndRecordsError()
    {
        var id = (await SeedMessagesAsync(1)).Single();

        await using (var db = fixture.CreateDbContext())
            await Processor(db, new RecordingPublisher(fail: true)).ProcessAsync(CancellationToken.None);

        var message = await LoadAsync(id);
        Assert.Null(message.ProcessedOnUtc);
        Assert.Equal(1, message.AttemptCount);
        Assert.Equal("Broker unavailable", message.Error);
    }

    [Fact]
    public async Task Process_AfterMaxAttempts_StopsRetryingPoisonMessage()
    {
        var id = (await SeedMessagesAsync(1)).Single();

        for (var attempt = 0; attempt < Options.MaxAttempts; attempt++)
        {
            await using var db = fixture.CreateDbContext();
            await Processor(db, new RecordingPublisher(fail: true)).ProcessAsync(CancellationToken.None);
        }

        var afterLimit = new RecordingPublisher();
        await using (var db = fixture.CreateDbContext())
            await Processor(db, afterLimit).ProcessAsync(CancellationToken.None);

        Assert.Empty(afterLimit.Published);
        Assert.Equal(Options.MaxAttempts, (await LoadAsync(id)).AttemptCount);
    }

    [Fact]
    public async Task FailedMessage_HoldsBackLaterMessagesWithSameKey_ButNotOtherKeys()
    {
        var ids = await SeedMessagesAsync(["order-A", "order-A", "order-B"]);
        var (placedA, paidA, placedB) = (ids[0], ids[1], ids[2]);
        var publisher = new RecordingPublisher(failWhen: m => m.MessageId == placedA);

        await using (var db = fixture.CreateDbContext())
            await Processor(db, publisher).ProcessAsync(CancellationToken.None);

        Assert.Equal([placedB], publisher.Published);
        Assert.Null((await LoadAsync(paidA)).ProcessedOnUtc);
        Assert.Equal(0, (await LoadAsync(paidA)).AttemptCount);
    }

    [Fact]
    public async Task HeldBackMessages_ArePublishedInOrderOnNextCycle()
    {
        var ids = await SeedMessagesAsync(["order-A", "order-A"]);

        await using (var db = fixture.CreateDbContext())
            await Processor(db, new RecordingPublisher(failWhen: m => m.MessageId == ids[0])).ProcessAsync(CancellationToken.None);

        var nextCycle = new RecordingPublisher();
        await using (var db = fixture.CreateDbContext())
            await Processor(db, nextCycle).ProcessAsync(CancellationToken.None);

        Assert.Equal(ids, nextCycle.Published.Reverse());
    }

    [Fact]
    public async Task PoisonMessage_BlocksLaterMessagesWithSameKey()
    {
        var ids = await SeedMessagesAsync(["order-A", "order-A", "order-B"], attemptCountOfFirst: Options.MaxAttempts);
        var publisher = new RecordingPublisher();

        await using (var db = fixture.CreateDbContext())
            await Processor(db, publisher).ProcessAsync(CancellationToken.None);

        Assert.Equal([ids[2]], publisher.Published);
    }

    [Fact]
    public async Task TwoProcessorsRunningConcurrently_PublishEachMessageExactlyOnce()
    {
        var ids = await SeedMessagesAsync(10);
        var publisher = new RecordingPublisher(delay: TimeSpan.FromMilliseconds(100));

        async Task RunProcessorAsync()
        {
            await using var db = fixture.CreateDbContext();
            await Processor(db, publisher).ProcessAsync(CancellationToken.None);
        }

        await Task.WhenAll(RunProcessorAsync(), RunProcessorAsync());

        Assert.Equal(ids.Count, publisher.Published.Count);
        Assert.Equal(ids.Order(), publisher.Published.Order());
    }
}
