namespace Confluent.Kafka;

/// <summary>
/// Extensions of <see cref="IConsumer{TKey, TValue}"/> to support tasks and concurrency.
/// </summary>
public static class ConsumerThreadingExtensions
{
    /// <summary>
    /// Consumes the next message/event asynchronously.
    /// </summary>
    /// <remarks>
    /// This implementation avoids unnecessary allocations and will not block on the thread pool.
    /// <para/>
    /// Prefer <see cref="ConsumeAllAsync{TKey, TValue}(IConsumer{TKey, TValue})"/> to reuse threading resources.
    /// </remarks>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <param name="consumer">The Kafka consumer.</param>
    /// <param name="cancellationToken">A token for cancellation.</param>
    /// <returns>A task for the consume result.</returns>
    public static ValueTask<ConsumeResult<TKey, TValue>> ConsumeAsync<TKey, TValue>(
        this IConsumer<TKey, TValue> consumer,
        CancellationToken cancellationToken = default)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));
#else
        if (consumer == null) throw new ArgumentNullException(nameof(consumer));
#endif

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = consumer.Consume(millisecondsTimeout: 0);

            if (result != null)
            {
                return new(result);
            }

            var task = Task.Factory.StartNew(
                static obj =>
                {
                    var (consumer, cancellationToken) = (Tuple<IConsumer<TKey, TValue>, CancellationToken>)obj!;

                    return consumer.Consume(cancellationToken) ??
                        throw new InvalidOperationException("Consumed a null result.");
                },
                Tuple.Create(consumer, cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
                TaskScheduler.Default);

            return new(task);
        }
#pragma warning disable CA1031 // Exceptions propagated through tasks.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            return GetFaultedTask<ConsumeResult<TKey, TValue>>(exception, cancellationToken);
        }
    }

    /// <summary>
    /// Creates an enumerable of messages/events for asynchronous iteration.
    /// </summary>
    /// <remarks>This implementation avoids unnecessary allocations and will not block on the thread pool.</remarks>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <param name="consumer">The Kafka consumer.</param>
    /// <returns>The consume result enumerable.</returns>
    public static IAsyncEnumerable<ConsumeResult<TKey, TValue>> ConsumeAllAsync<TKey, TValue>(
        this IConsumer<TKey, TValue> consumer)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));
#else
        if (consumer == null) throw new ArgumentNullException(nameof(consumer));
#endif

        return new ConsumerEnumerable<TKey, TValue>(consumer);
    }

    static ValueTask<T> GetFaultedTask<T>(
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException cancelException &&
            cancelException.CancellationToken == cancellationToken)
        {

#if NET5_0_OR_GREATER
            return ValueTask.FromCanceled<T>(cancellationToken);
#else
            return new(Task.FromCanceled<T>(cancellationToken));
#endif
        }

#if NET5_0_OR_GREATER
        return ValueTask.FromException<T>(exception);
#else
        return new(Task.FromException<T>(exception));
#endif
    }

    sealed class ConsumerEnumerable<TKey, TValue>(IConsumer<TKey, TValue> consumer) :
        IAsyncEnumerable<ConsumeResult<TKey, TValue>>
    {
        public IAsyncEnumerator<ConsumeResult<TKey, TValue>> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new ConsumerEnumerator<TKey, TValue>(consumer, cancellationToken);
        }
    }

    sealed class ConsumerEnumerator<TKey, TValue> : IAsyncEnumerator<ConsumeResult<TKey, TValue>>
    {
        const int IDLE = 0, ACTIVE = 1, DISPOSED = 2;

        readonly IConsumer<TKey, TValue> consumer;
        readonly CancellationToken cancellationToken;
        readonly ManualResetEvent waitGate = new(initialState: false);
        readonly Task waitTask;

        volatile ConsumeResult<TKey, TValue>? current;
        volatile TaskCompletionSource<bool>? waitResult;
        volatile int status;

        public ConsumerEnumerator(IConsumer<TKey, TValue> consumer, CancellationToken cancellationToken)
        {
            this.consumer = consumer;
            this.cancellationToken = cancellationToken;

            // Use a background thread so we don't block on the thread pool.
            waitTask = Task.Factory.StartNew(
                WaitLoop,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public ConsumeResult<TKey, TValue> Current
        {
            get
            {
                var current = this.current;

#if NET7_0_OR_GREATER
                ObjectDisposedException.ThrowIf(status == DISPOSED, this);
#else
                if (status == DISPOSED) throw new ObjectDisposedException(GetType().FullName);
#endif

                return current ?? throw new InvalidOperationException("Move has not occurred.");
            }
        }

        public ValueTask<bool> MoveNextAsync()
        {
            switch (Interlocked.CompareExchange(ref status, ACTIVE, IDLE))
            {
                case ACTIVE:
                    throw new InvalidOperationException("Move is already in progress.");

                case DISPOSED:
                    throw new ObjectDisposedException(GetType().FullName);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

#pragma warning disable CA1849 // Fast path: consume a buffered message (does not block).
                current = consumer.Consume(millisecondsTimeout: 0);
#pragma warning restore CA1849

                if (current != null)
                {
                    status = IDLE;
                    return new(true);
                }

                var waitResult = this.waitResult = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                waitGate.Set();

                return new(waitResult.Task);
            }
#pragma warning disable CA1031 // Exceptions propagated through tasks.
            catch (Exception exception)
#pragma warning restore CA1031
            {
                status = IDLE;
                return GetFaultedTask<bool>(exception, cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            switch (Interlocked.CompareExchange(ref status, DISPOSED, IDLE))
            {
                case ACTIVE:
                    throw new InvalidOperationException("Move is still in progress.");

                case IDLE:
                    waitGate.Set();
                    break;
            }

            await waitTask.ConfigureAwait(false);
            waitGate.Dispose();
        }

        void WaitLoop()
        {
            while (true)
            {
                waitGate.WaitOne();

                if (status == DISPOSED)
                {
                    break;
                }

                waitGate.Reset();

                var waitResult = this.waitResult!;
                this.waitResult = null;

                try
                {
                    // May block for extended periods.
                    current = consumer.Consume(cancellationToken) ??
                        throw new InvalidOperationException("Consumed a null result.");
                }
#pragma warning disable CA1031 // Exceptions propagated through tasks.
                catch (Exception exception)
#pragma warning restore CA1031
                {
                    status = IDLE;

                    if (exception is OperationCanceledException cancelException &&
                        cancelException.CancellationToken == cancellationToken)
                    {
#if NET5_0_OR_GREATER
                        waitResult.SetCanceled(this.cancellationToken);
#else
                        waitResult.SetCanceled();
#endif

                        break;
                    }

                    waitResult.SetException(exception);
                    continue;
                }

                status = IDLE;
                waitResult.SetResult(true);
            }

            current = null;
        }
    }
}
