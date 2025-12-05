namespace Confluent.Kafka.DependencyInjection.Tests;

using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public sealed class ConsumerThreadingExtensionsTests(TestContext context)
{
    [TestMethod]
    [DataRow(10, 1, 1)]
    [DataRow(24, 1, 4)]
    [Timeout(30_000, CooperativeCancellation = true)]
    public async Task InvokesProcessFuncForAllMessages(
        int totalMessages,
        int gapMillis,
        int parallelism,
        TimeProvider? time = null)
    {
        time ??= TimeProvider.System;

        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);

        var pending = new ConcurrentQueue<int>();
        var processed = new ConcurrentQueue<ConsumeResult<int?, int>>();
        var committed = new ConcurrentQueue<TopicPartitionOffset>();

        var broker = new FakeConsumer<int?, int>(
            timeout =>
            {
                if (timeout != TimeSpan.Zero)
                {
                    throw new Exception("Consume should not block.");
                }

                if (pending.TryDequeue(out var id))
                {
                    if (id == totalMessages)
                    {
                        stopping.Cancel();
                    }

                    var sample = Random.Shared.Next(parallelism);

                    return new ConsumeResult<int?, int>
                    {
                        Topic = "test-topic",
                        Partition = 99,
                        Offset = id,
                        LeaderEpoch = 9,
                        Message = new()
                        {
                            Key = sample == 0 ? default(int?) : sample,
                            Value = id,
                        },
                    };
                }

                return null;
            },
            committed.Enqueue);

        var concurrency = 0;
        var keyConcurrency = new ConcurrentDictionary<int, byte>();

        var processTask = broker.ConsumeAllAsync(
            async result =>
            {
                if (Interlocked.Increment(ref concurrency) > parallelism)
                {
                    throw new Exception("Parallelism exceeded.");
                }

                if (result.Message.Key != null && !keyConcurrency.TryAdd(result.Message.Key.Value, default))
                {
                    throw new Exception("Key concurrency violated.");
                }

                var interval = -Math.Log(Random.Shared.NextDouble()) * parallelism * gapMillis;

                if (interval >= 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(interval), time, context.CancellationToken);
                }

                processed.Enqueue(result);
                Interlocked.Decrement(ref concurrency);

                if (result.Message.Key != null)
                {
                    keyConcurrency.TryRemove(result.Message.Key.Value, out _);
                }
            },
            parallelism,
            storeOffsets: true,
            time: time,
            cancellationToken: stopping.Token);

        var emitTask = Task.Run(
            async () =>
            {
                for (var index = 1; index <= totalMessages; index++)
                {
                    var delay = -Math.Log(Random.Shared.NextDouble()) * gapMillis;

                    if (delay >= 1)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), time, context.CancellationToken);
                    }

                    pending.Enqueue(index);
                }
            },
            context.CancellationToken);

        try
        {
            await Task.WhenAll(processTask, emitTask);
        }
        catch (OperationCanceledException) when (!context.CancellationToken.IsCancellationRequested)
        {
        }

        CollectionAssert.AreEquivalent(
            Enumerable.Range(1, totalMessages).ToList(),
            processed.Select(x => x.Message.Value).ToList(),
            "Processed values did not match.");

        foreach (var keyGroup in processed.GroupBy(x => x.Message.Key, x => x.Message.Value))
        {
            if (keyGroup.Key != null)
            {
                CollectionAssert.AreEqual(
                    keyGroup.OrderBy(x => x).ToList(),
                    keyGroup.ToList(),
                    "Messages processed out of order");
            }
        }

        CollectionAssert.AreEqual(
            Enumerable.Range(2, totalMessages).ToList(),
            committed.Select(x => (int)x.Offset).ToList(),
            "Messages committed out of order");
    }

    [TestMethod]
    [DataRow(100, 100, 10)]
    [DataRow(10_000, 10, 10)]
    [DataRow(1_000, 10, 100)]
    [Timeout(30_000, CooperativeCancellation = true)]
    public Task InvokesProcessFuncForAllMessages_WithFakeTime(int totalMessages, int gapMillis, int parallelism)
    {
        var time = new FakeTimeProvider();
        var processTask = InvokesProcessFuncForAllMessages(totalMessages, gapMillis, parallelism, time);

        var timeTask = Task.Run(
            async () =>
            {
                while (!processTask.IsCompleted)
                {
                    await Task.Yield();
                    time.Advance(TimeSpan.FromMilliseconds(gapMillis));
                }

                context.WriteLine("Time advanced {0}.", time.GetUtcNow() - time.Start);
            },
            context.CancellationToken);

        return Task.WhenAll(processTask, timeTask);
    }
}
