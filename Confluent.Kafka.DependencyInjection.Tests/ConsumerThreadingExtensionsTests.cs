namespace Confluent.Kafka.DependencyInjection.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public sealed class ConsumerThreadingExtensionsTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task ConsumeAsyncPropagatesResult(bool enumerate, bool wait)
    {
        var expected = new ConsumeResult<object?, object?>();

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                if (timeout == TimeSpan.Zero && wait)
                {
                    return null;
                }

                return expected;
            });

        var actual = await ConsumeOne(consumer, enumerate);
        Assert.AreEqual(expected, actual, "Unexpected consume result.");
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task ConsumeAsyncPropagatesException(bool enumerate, bool wait)
    {
        var expected = new ConsumeException(new(), new(ErrorCode.Local_Application));

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                if (timeout == TimeSpan.Zero && wait)
                {
                    return null;
                }

                throw expected;
            });

        var actual = await Assert.ThrowsAsync<ConsumeException>(() => ConsumeOne(consumer, enumerate));
        Assert.AreEqual(expected, actual, "Unexpected consume exception.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ConsumeAsyncPropagatesCancellation(bool enumerate)
    {
        var cancellation = new CancellationTokenSource();

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                if (timeout == TimeSpan.Zero)
                {
                    return null;
                }

                cancellation.Cancel();
                throw new OperationCanceledException(cancellationToken);
            });

        var actual = await Assert.ThrowsAsync<OperationCanceledException>(
            () => ConsumeOne(consumer, enumerate, cancellation.Token));

        Assert.AreEqual(cancellation.Token, actual.CancellationToken, "Unexpected cancellation token.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ConsumeAsyncChecksForABufferedMessage(bool enumerate)
    {
        var callingThreadId = Environment.CurrentManagedThreadId;
        var capturedTimeout = default(TimeSpan?);
        var capturedThreadId = default(int?);

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                capturedTimeout ??= timeout;
                capturedThreadId ??= Environment.CurrentManagedThreadId;

                return new();
            });

        await ConsumeOne(consumer, enumerate);

        Assert.AreEqual(TimeSpan.Zero, capturedTimeout, "Consume(...) received an unexpected timeout.");
        Assert.AreEqual(callingThreadId, capturedThreadId, "Consume(...) not invoked on the calling thread.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ConsumeAsyncWaitsOnBackgroundThread(bool enumerate)
    {
        var callingThreadId = Environment.CurrentManagedThreadId;
        var capturedThreadId = default(int?);
        var capturedThreadIsPooled = default(bool?);

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                if (timeout == TimeSpan.Zero)
                {
                    return null;
                }

                capturedThreadId ??= Environment.CurrentManagedThreadId;
                capturedThreadIsPooled ??= Thread.CurrentThread.IsThreadPoolThread;

                return new();
            });

        await ConsumeOne(consumer, enumerate);

        Assert.AreNotEqual(callingThreadId, capturedThreadId, "Consume(...) invoked on calling thread.");
        Assert.IsFalse(capturedThreadIsPooled, "Consume(...) invoked on thread pool.");
    }

    [TestMethod]
    public async Task ConsumeAllAsyncReusesBackgroundThread()
    {
        var capturedThreadIds = new HashSet<int>();

        var consumer = new FakeConsumer(
            (timeout, cancellationToken) =>
            {
                if (timeout == TimeSpan.Zero)
                {
                    return null;
                }

                capturedThreadIds.Add(Environment.CurrentManagedThreadId);

                return new();
            });

        await using var enumerator = consumer.ConsumeAllAsync().GetAsyncEnumerator();

        for (var i = 0; i < 3; i++)
        {
            await enumerator.MoveNextAsync();
        }

        Assert.AreEqual(1, capturedThreadIds.Count, "Consume(...) invoked on multiple threads.");
    }

    static async Task<ConsumeResult<object?, object?>> ConsumeOne(
        IConsumer<object?, object?> consumer,
        bool enumerate,
        CancellationToken cancellationToken = default)
    {
        if (enumerate)
        {
            await using var enumerator = consumer.ConsumeAllAsync().GetAsyncEnumerator(cancellationToken);
            await enumerator.MoveNextAsync();
            return enumerator.Current;
        }

        return await consumer.ConsumeAsync(cancellationToken);
    }
}
