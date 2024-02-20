namespace Confluent.Kafka.Hosting;

using Confluent.Kafka;

using System.Collections.Concurrent;

/// <summary>
/// A base implementation of <see cref="IHostedConsumer"/> using <see cref="ParallelConsumeOptions"/>.
/// </summary>
/// <typeparam name="TKey">The consumer key type.</typeparam>
/// <typeparam name="TValue">The consumer value type.</typeparam>
public abstract class ParallelConsumer<TKey, TValue> : IHostedConsumer
{
    /// <summary>
    /// Gets the options for parallel consuming.
    /// </summary>
    protected abstract ParallelConsumeOptions Options { get; }

    /// <summary>
    /// Gets the Kafka consumer client to retrieve new messages/events.
    /// </summary>
    protected abstract IConsumer<TKey, TValue> Client { get; }

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Options.MaxDegreeOfParallelism is 0)
        {
            return;
        }

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var exceptions = new ConcurrentBag<Exception>();

#if NET6_0_OR_GREATER
        var completion = new TaskCompletionSource();
#else
        var completion = new TaskCompletionSource<byte>();
#endif

        var totalCount = 0;
        var next = default(ConsumeResult<TKey, TValue>);

        var consumeSlow = default(ThreadStart);
        consumeSlow = new ThreadStart(() => ConsumeLoop(blocking: true));

        ConsumeLoop();
        await completion.Task.ConfigureAwait(false);

#pragma warning disable CA1031 // General exceptions propagated through task.
        void ConsumeLoop(bool blocking = false)
        {
            try
            {
                var paused = false;

                if (blocking)
                {
                    next = Client.Consume(cancellation.Token);
                    TryProcess(ref paused);
                }

                while (!paused && !cancellation.IsCancellationRequested)
                {
                    var buffered = Client.Consume(millisecondsTimeout: 0);

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
                    Complete();
                }
            }
        }

        void TryProcess(ref bool paused)
        {
            if (Interlocked.Increment(ref totalCount) > Options.MaxDegreeOfParallelism)
            {
                // Pause when we've reached our parallelism limit.
                paused = true;
            }
            else
            {
                Process(next);
            }
        }

        async void Process(ConsumeResult<TKey, TValue> result)
        {
            try
            {
                await ProcessAsync(result, cancellation.Token).ConfigureAwait(false);
            }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
                cancellation.Cancel();
            }

            if (Interlocked.Decrement(ref totalCount) == Options.MaxDegreeOfParallelism)
            {
                Process(next);
                ConsumeLoop();
            }
            else if (cancellation.IsCancellationRequested && Volatile.Read(ref next) is null && totalCount is 0)
            {
                Complete();
            }
        }

        void Complete()
        {
            if (exceptions.IsEmpty)
            {
#if NET6_0_OR_GREATER
                completion.SetCanceled(cancellationToken);
#else
                completion.SetCanceled();
#endif
            }
            else
            {
                completion.SetException(exceptions);
            }
#pragma warning restore CA1031
        }
    }

    /// <summary>
    /// Processes a Kafka consume result asynchronously.
    /// </summary>
    /// <param name="result">A result containing the consumed message/event.</param>
    /// <param name="cancellationToken">A token for cancellation.</param>
    /// <returns>A task for the process operation.</returns>
    protected abstract ValueTask ProcessAsync(ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken);
}
