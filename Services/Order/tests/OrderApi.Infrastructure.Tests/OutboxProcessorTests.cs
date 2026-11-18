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

    private sealed class RecordingPublisher(TimeSpan delay = default, bool fail = false) : IMessagePublisher
    {
        public ConcurrentBag<Guid> Published { get; } = [];

        public async Task PublishAsync(Guid messageId, string type, string content, CancellationToken cancellationToken)
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            if (fail)
                throw new InvalidOperationException("Broker unavailable");

            Published.Add(messageId);
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

    private async Task<List<Guid>> SeedMessagesAsync(int count)
    {
        var messages = Enumerable.Range(0, count)
            .Select(i => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "TestEvent",
                Content = "{}",
                OccurredOnUtc = DateTime.UtcNow.AddSeconds(i)
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
