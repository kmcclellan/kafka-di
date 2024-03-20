namespace Confluent.Kafka.Hosting;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// A base <see cref="IHostedService"/> for Kafka consuming.
/// </summary>
public abstract class ConsumerService<TKey, TValue> : BackgroundService
{
    readonly IConsumerProcessor<TKey, TValue> processor;
    readonly ILogger? logger;

    /// <summary>
    /// Initializes the service with the given processor and logger.
    /// </summary>
    /// <param name="processor">The consumer processor.</param>
    /// <param name="logger">A logger to use for the service.</param>
    protected ConsumerService(IConsumerProcessor<TKey, TValue> processor, ILogger? logger = null)
    {
        this.processor = processor;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the Kafka client options.
    /// </summary>
    protected abstract KafkaClientOptions ClientOptions { get; }

    /// <summary>
    /// Gets the options for hosted consuming.
    /// </summary>
    protected abstract HostedConsumeOptions ConsumeOptions { get; }

    /// <summary>
    /// Consumes Kafka messages until stopped.
    /// </summary>
    /// <param name="stoppingToken">A token to signal when the host is stopping.</param>
    /// <returns>A task for the long-running consume operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (ConsumeOptions.MaxDegreeOfParallelism == 0)
        {
            logger?.LogInformation(
                $"Consumer service is disabled ({nameof(HostedConsumeOptions.MaxDegreeOfParallelism)}: 0).");

            return;
        }

        using var consumer = ClientOptions.CreateConsumer<TKey, TValue>();
        consumer.Subscribe(ConsumeOptions.Topics);

        try
        {
            await consumer.ConsumeAllAsync(processor, ConsumeOptions, logger, stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            consumer.Close();
        }
    }
}
