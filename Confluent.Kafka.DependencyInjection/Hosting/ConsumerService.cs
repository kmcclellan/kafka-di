namespace Confluent.Kafka.Hosting
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes from Kafka as a hosted service.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    public abstract class ConsumerService<TKey, TValue> : BackgroundService
    {
        static readonly Action<ILogger, Exception> LogDisabled =
            LoggerMessage.Define(LogLevel.Information, default, "Hosted consumer is disabled");

        static readonly Action<ILogger, Exception> LogNoAssignment =
            LoggerMessage.Define(LogLevel.Warning, default, "No subscription/assignment for hosted consumer");

        static readonly Func<ILogger, string, string, int, long, IDisposable> LogOffset =
            LoggerMessage.DefineScope<string, string, int, long>(
                "{KafkaClient} - {KafkaTopic} [{KafkaPartition}] @{KafkaOffset}");

        static readonly Action<ILogger, double, Exception> LogReceived =
            LoggerMessage.Define<double>(
                LogLevel.Information,
                new EventId(50, "MessageConsumed"),
                "Kafka consume result received ({LagSeconds:N1}s lag)");

        static readonly Action<ILogger, long, Exception> LogProcessed =
            LoggerMessage.Define<long>(
                LogLevel.Information,
                new EventId(51, "MessageProcessed"),
                "Kafka consume result processed ({ElapsedMilliseconds:N0}ms)");

        static readonly Action<ILogger, string, ErrorCode, Exception> LogError =
            LoggerMessage.Define<string, ErrorCode>(
                LogLevel.Error,
                default,
                "[{KafkaClient}] Error '{ErrorCode}' in hosted consumer");

        readonly IConsumer<TKey, TValue> consumer;
        readonly IOptions<ConsumerHostingOptions> options;
        readonly ILogger<ConsumerService<TKey, TValue>> logger;
        readonly IEqualityComparer<TKey> keyComparer;

        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="options">Options for hosted consuming.</param>
        /// <param name="logger">A logger for information and errors while consuming.</param>
        /// <param name="keyComparer">A custom comparer for message keys.</param>
        protected ConsumerService(
            IConsumer<TKey, TValue> consumer,
            IOptions<ConsumerHostingOptions> options,
            ILogger<ConsumerService<TKey, TValue>> logger,
            IEqualityComparer<TKey> keyComparer = null)
        {
            this.consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.keyComparer = keyComparer;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (options.Value.Disabled)
            {
                LogDisabled(logger, null);
                return;
            }

            stoppingToken.ThrowIfCancellationRequested();

            if (options.Value.Assignment.Count > 0)
            {
                consumer.Assign(options.Value.Assignment);
            }
            else if (options.Value.Subscription.Count > 0)
            {
                consumer.Subscribe(options.Value.Subscription);
            }
            else
            {
                LogNoAssignment(logger, null);
                return;
            }

            try
            {
                while (true)
                {
                    try
                    {
                        var consumeTask = consumer.ConsumeAllAsync(
                            async result =>
                            {
                                using (LogOffset(logger, consumer.Name, result.Topic, result.Partition, result.Offset))
                                {
                                    var timestamp = DateTime.UtcNow;

                                    if (logger.IsEnabled(LogLevel.Information))
                                    {
                                        var lag = timestamp - result.Message.Timestamp.UtcDateTime;
                                        LogReceived(logger, lag.TotalSeconds, null);
                                    }

                                    // Not safe to cancel multiple messages since we cannot commit individually.
                                    var cancellationToken = options.Value.MaxDegreeOfParallelism == 1
                                        ? stoppingToken
                                        : CancellationToken.None;

                                    await ProcessAsync(result, cancellationToken).ConfigureAwait(false);

                                    if (logger.IsEnabled(LogLevel.Information))
                                    {
                                        var elapsed = DateTime.UtcNow - timestamp;
                                        LogProcessed(logger, (long)elapsed.TotalMilliseconds, null);
                                    }
                                }
                            },
                            options.Value.MaxDegreeOfParallelism,
                            options.Value.StoreProcessedOffsets,
                            keyComparer,
                            cancellationToken: stoppingToken);

                        await consumeTask.ConfigureAwait(false);
                    }
                    catch (KafkaException exception) when (HandleException(exception))
                    {
                    }
                }
            }
            finally
            {
                if (options.Value.Assignment.Count > 0)
                {
                    consumer.Unassign();
                }
                else
                {
                    consumer.Unsubscribe();
                }
            }
        }

        /// <summary>
        /// Processes a consume result asynchronously.
        /// </summary>
        /// <param name="result">The consume result.</param>
        /// <param name="cancellationToken">A token for cancellation.</param>
        /// <returns>A task for the asynchronous operation.</returns>
        protected abstract ValueTask ProcessAsync(
            ConsumeResult<TKey, TValue> result,
            CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to handle/recover from a Kafka exception.
        /// </summary>
        /// <remarks>
        /// The base implementation logs the error and resumes unless <see cref="Error.IsFatal"/>.
        /// </remarks>
        /// <param name="exception">The Kafka exception.</param>
        /// <returns>
        /// <see langword="true"/> to resume consuming, <see langword="false"/> to propagate the exception to the host.
        /// </returns>
        protected virtual bool HandleException(KafkaException exception)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(exception, nameof(exception));
#else
            if (exception == null) throw new ArgumentNullException(nameof(exception));
#endif

            LogError(logger, consumer.Name, exception.Error.Code, exception);
            return !exception.Error.IsFatal;
        }
    }
}
