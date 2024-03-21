namespace Tests;

using Confluent.Kafka;
using Confluent.Kafka.Hosting;

using Microsoft.Extensions.Logging.Debug;

[TestClass]
public class ParallelConsumerTests : IDisposable
{
    readonly FakeConsumer<int, object?> consumer = new();
    readonly FakeConsumerProcessor<int, object?> processor = new();
    readonly ParallelConsumeOptions options = new();
    readonly DebugLoggerProvider loggers = new();
    readonly CancellationTokenSource cancellation = new();

    [TestMethod]
    public void ProcessMultipleResultsSynchronously()
    {
        options.MaxDegreeOfParallelism = 1;
        var results = new ConsumeResult<int, object?>[10];

        for (var i = 0; i < results.Length; i++)
        {
            var result = results[i] = new ConsumeResult<int, object?> { Offset = i };

            consumer.Setup(result);
            processor.Setup(result, Task.CompletedTask);
        }

        consumer.Setup(
            new OperationCanceledException(cancellation.Token),
            TimeSpan.FromMilliseconds(1),
            cancellation.Cancel);

        TestConsume();

        foreach (var result in results)
        {
            processor.Verify(result);
        }
    }

    [TestMethod]
    public void ProcessMultipleResultsAsynchronously()
    {
        var results = new ConsumeResult<int, object?>[options.MaxDegreeOfParallelism = 10];
        var completions = new TaskCompletionSource[results.Length];

        for (var i = 0; i < results.Length; i++)
        {
            var result = results[i] = new ConsumeResult<int, object?> { Offset = i };
            completions[i] = new();

            consumer.Setup(result);
            processor.Setup(result, completions[i].Task);
        }

        consumer.Setup(
            new OperationCanceledException(cancellation.Token),
            TimeSpan.FromMilliseconds(1),
            () =>
            {
                for (var i = results.Length - 1; i >= 0; i--)
                {
                    completions[i].SetResult();
                }

                cancellation.Cancel();
            });

        TestConsume();

        for (var i = results.Length - 1; i >= 0; i--)
        {
            processor.Verify(results[i]);
        }
    }

    void TestConsume()
    {
        var consumeTask = consumer.Instance.ConsumeAllAsync(
            processor.Instance,
            options,
            loggers.CreateLogger(nameof(ParallelConsumerTests)),
            cancellation.Token);

        try
        {
            consumeTask.Wait();
        }
        catch (AggregateException exception)
        {
            Assert.AreEqual(1, exception.InnerExceptions.Count);
            Assert.IsInstanceOfType<OperationCanceledException>(exception.InnerException);

            var cancelEx = (OperationCanceledException)exception.InnerException;
            Assert.AreEqual(cancellation.Token, cancelEx.CancellationToken);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        loggers.Dispose();
        cancellation.Dispose();
    }
}
