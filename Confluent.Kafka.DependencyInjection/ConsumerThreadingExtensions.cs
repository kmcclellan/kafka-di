namespace Confluent.Kafka
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions of <see cref="IConsumer{TKey, TValue}"/> to support tasks and concurrency.
    /// </summary>
    public static class ConsumerThreadingExtensions
    {
        static readonly TimeSpan PollMinDelay = TimeSpan.FromMilliseconds(10),
            PollMaxDelay = TimeSpan.FromMilliseconds(160);

        /// <summary>
        /// Consumes the next message/event asynchronously.
        /// </summary>
        /// <remarks>
        /// This implementation avoids unnecessary allocations and thread blocking.
        /// </remarks>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="time">A time provider for polling delays, or <see langword="null"/> to use system time.</param>
        /// <param name="cancellationToken">A token for cancellation.</param>
        /// <returns>A task for the consume result.</returns>
        public static async ValueTask<ConsumeResult<TKey, TValue>> ConsumeAsync<TKey, TValue>(
            this IConsumer<TKey, TValue> consumer,
            TimeProvider time = null,
            CancellationToken cancellationToken = default)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));
#else
            if (consumer == null) throw new ArgumentNullException(nameof(consumer));
#endif

            cancellationToken.ThrowIfCancellationRequested();

            ConsumeResult<TKey, TValue> result;
            var delay = PollMinDelay;

            while ((result = consumer.Consume(millisecondsTimeout: 0)) == null)
            {
                if (time == null)
                {
                    time = TimeProvider.System;
                }

                await time.Delay(delay, cancellationToken).ConfigureAwait(false);

                if (delay < PollMaxDelay)
                {
                    delay = new TimeSpan(delay.Ticks * 2);
                }
            }

            return result;
        }

        /// <summary>
        /// Consumes messages indefinitely by invoking the given delegate.
        /// </summary>
        /// <remarks>
        /// Messages with the same key are processed sequentially regardless of parallelism.
        /// </remarks>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="processFunc">The delegate to process each result.</param>
        /// <param name="maxDegreeOfParallelism">The maximum number of messages to process concurrently.</param>
        /// <param name="storeOffsets">Whether to store offsets manually for processed messages.</param>
        /// <param name="keyComparer">A comparer for message keys, or <see langword="null"/> to use a default.</param>
        /// <param name="time">A time provider for polling delays, or <see langword="null"/> to use system time.</param>
        /// <param name="cancellationToken">A token for cancellation.</param>
        /// <returns>A task for the consume result.</returns>
        public static Task ConsumeAllAsync<TKey, TValue>(
            this IConsumer<TKey, TValue> consumer,
            Func<ConsumeResult<TKey, TValue>, ValueTask> processFunc,
            int maxDegreeOfParallelism = 1,
            bool storeOffsets = false,
            IEqualityComparer<TKey> keyComparer = null,
            TimeProvider time = null,
            CancellationToken cancellationToken = default)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));
            ArgumentNullException.ThrowIfNull(processFunc, nameof(processFunc));
#else
            if (consumer == null) throw new ArgumentNullException(nameof(consumer));
            if (processFunc == null) throw new ArgumentNullException(nameof(processFunc));
#endif

            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxDegreeOfParallelism),
                    maxDegreeOfParallelism,
                    $"parallelism ('{maxDegreeOfParallelism}') must be greater than or equal to one.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (time == null)
            {
                time = TimeProvider.System;
            }

            if (maxDegreeOfParallelism == 1)
            {
                return ConsumeSequential();
            }

            var totalInFlight = 1;
            var completion = new TaskCompletionSource<byte>();
            var exceptions = new ConcurrentBag<Exception>();

            if (keyComparer == null)
            {
                if (ByteComparer.Instance is IEqualityComparer<TKey> byteComparer)
                {
                    keyComparer = byteComparer;
                }
                else
                {
                    keyComparer = EqualityComparer<TKey>.Default;
                }
            }

            var buffer = new ConsumeBuffer<TKey, TValue>(maxDegreeOfParallelism, keyComparer);

            StartPolling();

            async Task ConsumeSequential()
            {
                while (true)
                {
                    var message = await ConsumeAsync(consumer, time, cancellationToken).ConfigureAwait(false);
                    await processFunc(message).ConfigureAwait(false);

                    if (storeOffsets)
                    {
                        consumer.StoreOffset(message);
                    }
                }
            }

#pragma warning disable CA1031 // Propagate exceptions through tasks.
            async void StartPolling()
            {
                while (true)
                {
                    var result = default(ConsumeResult<TKey, TValue>);

                    try
                    {
                        var delay = PollMinDelay;

                        while ((result = consumer.Consume(millisecondsTimeout: 0)) == null && exceptions.IsEmpty)
                        {
                            await time.Delay(delay, cancellationToken).ConfigureAwait(false);

                            if (delay < PollMaxDelay)
                            {
                                delay = new TimeSpan(delay.Ticks * 2);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(exception);
                    }

                    if (result != null)
                    {
                        var index = buffer.TryStart(result);
                        var incremented = Interlocked.Increment(ref totalInFlight);

                        if (index != -1)
                        {
                            ProcessMessage(index);
                        }

                        if (incremented > maxDegreeOfParallelism)
                        {
                            // Let the next completing message resume.
                            break;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested || !exceptions.IsEmpty)
                    {
                        if (Interlocked.Decrement(ref totalInFlight) == 0)
                        {
                            Complete();
                        }

                        break;
                    }
                }
            }

            async void ProcessMessage(int index)
            {
                try
                {
                    await processFunc(buffer[index]).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }

                var nextToProcess = buffer.MarkProcessed(index, out var commit);

                if (nextToProcess != -1)
                {
                    ProcessMessage(nextToProcess);
                }

                if (commit)
                {
                    int decremented;

                    do
                    {
                        if (storeOffsets && exceptions.IsEmpty)
                        {
                            try
                            {
                                consumer.StoreOffset(buffer[index]);
                            }
                            catch (Exception exception)
                            {
                                exceptions.Add(exception);
                            }
                        }

                        index = buffer.MarkFinished();
                        decremented = Interlocked.Decrement(ref totalInFlight);

                        if (decremented == maxDegreeOfParallelism)
                        {
                            if (cancellationToken.IsCancellationRequested || !exceptions.IsEmpty)
                            {
                                decremented = Interlocked.Decrement(ref totalInFlight);
                            }
                            else
                            {
                                StartPolling();
                            }
                        }
                    }
                    while (index != -1);

                    if (decremented == 0)
                    {
                        Complete();
                    }
                }
            }
#pragma warning restore CA1031

            void Complete()
            {
                if (exceptions.IsEmpty)
                {
#if NET5_0_OR_GREATER
                    completion.SetCanceled(cancellationToken);
#else
                    completion.SetCanceled();
#endif
                }
                else
                {
                    completion.SetException(exceptions);
                }
            }

            return completion.Task;
        }

        /// <summary>
        /// A queue supporting concurrency, indexed access, and key-based processing constraints.
        /// </summary>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        sealed class ConsumeBuffer<TKey, TValue>
        {
            readonly IEqualityComparer<TKey> keyComparer;
            readonly ConsumeResult<TKey, TValue>[] results;
            readonly int[] committing;
            readonly int[] blocking;
            readonly int?[] hashes;

            int head, tail, count;

            public ConsumeBuffer(int size, IEqualityComparer<TKey> keyComparer)
            {
                this.keyComparer = keyComparer;
                results = new ConsumeResult<TKey, TValue>[size];
                committing = new int[size];
                blocking = new int[size];
                hashes = new int?[size];
            }

            public ConsumeResult<TKey, TValue> this[int index] => results[index];

            /// <summary>
            /// Stores the result and determines if processing may start.
            /// </summary>
            /// <param name="result">The result.</param>
            /// <returns>The index of the result within the buffer, or <c>-1</c> if processing is blocked.</returns>
            public int TryStart(ConsumeResult<TKey, TValue> result)
            {
                var index = tail++;

                if (tail == results.Length)
                {
                    tail = 0;
                }

                results[index] = result;
                committing[index] = Interlocked.Increment(ref count) == 1 ? 1 : 0;
                blocking[index] = -1;

                if (result.Message != null && result.Message.Key != null)
                {
                    hashes[index] = keyComparer.GetHashCode(result.Message.Key);

                    for (var i = 1; i < count; i++)
                    {
                        var prev = index - i;

                        if (prev < 0)
                        {
                            prev += results.Length;
                        }

                        if (hashes[prev] == hashes[index] &&
                            keyComparer.Equals(results[prev].Message.Key, result.Message.Key))
                        {
                            if (Interlocked.CompareExchange(ref blocking[prev], index, -1) == -1)
                            {
                                return -1;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    hashes[index] = null;
                }

                return index;
            }

            /// <summary>
            /// Updates the status of a processed result and determines if it can be committed,
            /// as well as the next result to process.
            /// </summary>
            /// <param name="index">The index of the processed result.</param>
            /// <param name="commit">Whether the processed result is safe to commit.</param>
            /// <returns>The index of the next result ready for processing, or <c>-1</c> if none exists.</returns>
            public int MarkProcessed(int index, out bool commit)
            {
                commit = Interlocked.Exchange(ref committing[index], -1) == 1;
                return Interlocked.Exchange(ref blocking[index], -2);
            }

            /// <summary>
            /// Removes a result from the head of the buffer and determines the next result to remove.
            /// </summary>
            /// <returns>The index of the next result ready for removal, or <c>-1</c> if none exists.</returns>
            public int MarkFinished()
            {
                results[head++] = null;

                if (head == results.Length)
                {
                    head = 0;
                }

                if (Interlocked.Decrement(ref count) > 0 &&
                    Interlocked.CompareExchange(ref committing[head], 1, 0) == -1)
                {
                    return head;
                }

                return -1;
            }
        }

        sealed class ByteComparer : IEqualityComparer<byte[]>
        {
            ByteComparer()
            {
            }

            public static ByteComparer Instance { get; } = new ByteComparer();

            public bool Equals(byte[] x, byte[] y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return x.AsSpan().SequenceEqual(y);
            }

            public int GetHashCode(byte[] obj)
            {
#if NET6_0_OR_GREATER
                var code = new HashCode();
                code.AddBytes(obj);

                return code.ToHashCode();
#else
                // https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
                const int fnvPrime = 16777619;
                const int fnvOffsetBasis = unchecked((int)2166136261);

                var hash = fnvOffsetBasis;

                foreach (var val in obj)
                {
                    hash *= fnvPrime;
                    hash ^= val;
                }

                return hash;
#endif
            }
        }
    }
}
