namespace Confluent.Kafka.Hosting;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Extensions of <see cref="IConsumer{TKey, TValue}"/> for processing messages/events in parallel.
/// </summary>
public static class ParallelConsumerExtensions
{
    static readonly Func<ILogger, string, int, long, IDisposable?> LogOffset =
        LoggerMessage.DefineScope<string, int, long>(
            "{KafkaTopic} [{KafkaPartition}] @{KafkaOffset}");

    static readonly Action<ILogger, int, Exception?> LogError =
        LoggerMessage.Define<int>(LogLevel.Warning, default, "Error {ErrorCode} occurred in hosted consumer");

    static readonly Action<ILogger, double?, Exception?> LogReceive =
        LoggerMessage.Define<double?>(
            LogLevel.Information,
            new(30, "ConsumeReceived"),
            "Consume result received (+{LagSeconds:N1}s)");

    static readonly Action<ILogger, double, Exception?> LogProcess =
        LoggerMessage.Define<double>(
            LogLevel.Information,
            new(31, "ConsumeProcessed"),
            "Consume result processed in {ElapsedMilliseconds:N0}ms");

    static readonly Action<ILogger, Exception?> LogPause =
        LoggerMessage.Define(LogLevel.Debug, default, "Pausing consume loop");

    static readonly Action<ILogger, Exception?> LogResume =
        LoggerMessage.Define(LogLevel.Debug, default, "Resuming consume loop");

    /// <summary>
    /// Consumes Kafka messages/events indefinitely.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <param name="consumer">The Kafka consumer.</param>
    /// <param name="processor">The processor for messages/events.</param>
    /// <param name="parallelOptions">Options for parallel consuming.</param>
    /// <param name="logger">
    /// A logger to report consumer errors and other occurrences (messages received, processed, etc.).
    /// </param>
    /// <param name="cancellationToken">A token for cancellation.</param>
    /// <returns>A task for the long-running consume operation (will only complete if canceled/faulted).</returns>
    public static async Task ConsumeAllAsync<TKey, TValue>(
        this IConsumer<TKey, TValue> consumer,
        IConsumerProcessor<TKey, TValue> processor,
        ParallelConsumeOptions? parallelOptions = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        parallelOptions ??= new();
        logger ??= NullLogger.Instance;

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var exceptions = new ConcurrentBag<Exception>();
        var completion = new TaskCompletionSource<byte>();

        var totalCount = 0;
        var next = default(ConsumeResult<TKey, TValue>);

        var consumeSlow = default(ThreadStart);
        consumeSlow = new(() => ConsumeLoop(blocking: true));

        ConsumeLoop();
        await completion.Task.ConfigureAwait(false);

        Debug.Assert(!exceptions.IsEmpty || cancellationToken.IsCancellationRequested, "Must be cancelled or faulted.");

        throw exceptions.IsEmpty
            ? new OperationCanceledException(cancellationToken)
            : new AggregateException(exceptions);

        void ConsumeLoop(bool blocking = false)
        {
            try
            {
                var paused = false;

                if (blocking)
                {
                    next = SafeConsume(blocking) ?? throw new InvalidOperationException("Unexpected null result.");
                    TryProcess(ref paused);
                }

                while (!paused && !cancellation.IsCancellationRequested)
                {
                    var buffered = SafeConsume(blocking: false);

                    if (buffered is null)
                    {
                        new Thread(consumeSlow!).Start();
                        return;
                    }

                    next = buffered;
                    TryProcess(ref paused);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
                cancellation.Cancel();
            }

            if (cancellation.IsCancellationRequested)
            {
                Volatile.Write(ref next, null);

                if (Volatile.Read(ref totalCount) is 0)
                {
                    completion.SetResult(default);
                }
            }
        }

        ConsumeResult<TKey, TValue>? SafeConsume(bool blocking)
        {
            while (true)
            {
                try
                {
                    return blocking ? consumer.Consume(cancellation.Token) : consumer.Consume(millisecondsTimeout: 0);
                }
                catch (KafkaException exception) when (exception.Error is { IsFatal: false })
                {
                    LogError(logger, (int)exception.Error.Code, exception);
                }
            }
        }

        void TryProcess(ref bool paused)
        {
            if (Interlocked.Increment(ref totalCount) > parallelOptions.MaxDegreeOfParallelism)
            {
                // Pause when we've reached our parallelism limit.
                LogPause(logger, null);
                paused = true;
            }
            else
            {
                Process(next);
            }
        }

        async void Process(ConsumeResult<TKey, TValue> result)
        {
            using (LogOffset(logger, result.Topic, result.Partition, result.Offset))
            {
                var startTime = DateTimeOffset.UtcNow;

                if (logger.IsEnabled(LogLevel.Information))
                {
                    var lag = result.Message is not null
                        ? startTime.ToUnixTimeMilliseconds() - result.Message.Timestamp.UnixTimestampMs
                        : default(double?);

                    LogReceive(logger, lag / 1000, null);
                }

                try
                {
                    await processor.ProcessAsync(result, cancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                    cancellation.Cancel();
                }

                if (logger.IsEnabled(LogLevel.Information))
                {
                    LogProcess(logger, DateTimeOffset.UtcNow.Subtract(startTime).TotalMilliseconds, null);
                }
            }

            if (Interlocked.Decrement(ref totalCount) == parallelOptions.MaxDegreeOfParallelism)
            {
                LogResume(logger, null);
                Process(next);
                ConsumeLoop();
            }
            else if (cancellation.IsCancellationRequested && Volatile.Read(ref next) is null && totalCount is 0)
            {
                completion.SetResult(default);
            }
        }
    }
}
