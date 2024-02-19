namespace Confluent.Kafka.Hosting;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// An <see cref="IHostedService"/> implementation for use with <see cref="IHostedConsumer"/>.
/// </summary>
public class HostedConsumerService : BackgroundService
{
    static readonly Action<ILogger, int?, Exception> LogKafkaError =
        LoggerMessage.Define<int?>(LogLevel.Warning, default, "Error {ErrorCode} occurred in hosted consumer");

    static readonly Action<ILogger, Exception> LogFatalException =
        LoggerMessage.Define(LogLevel.Error, default, "Unhandled exception in hosted consumer");

    readonly IEnumerable<IHostedConsumer> consumers;
    readonly ILogger<HostedConsumerService>? logger;

    /// <summary>
    /// Initializes the service with the given consumers and logger.
    /// </summary>
    /// <param name="consumers">The hosted consumers.</param>
    /// <param name="logger">A logger for the service.</param>
    public HostedConsumerService(IEnumerable<IHostedConsumer> consumers, ILogger<HostedConsumerService>? logger = null)
    {
        this.consumers = consumers;
        this.logger = logger;
    }

#pragma warning disable CA1031 // General exceptions propagated through tasks.

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var faultOrCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var consumeTasks = new List<Task>();

        foreach (var consumer in consumers)
        {
            Task consumeTask;

            try
            {
                consumeTask = ConsumeLoop(consumer, faultOrCancellation.Token);
            }
            catch (Exception exception)
            {
                consumeTask = Task.FromException(exception);
            }
        }

        if (consumeTasks.Count > 0)
        {
            var firstTask = await Task.WhenAny(consumeTasks).ConfigureAwait(false);

            if (firstTask.IsFaulted)
            {
                if (logger is not null && logger.IsEnabled(LogLevel.Error))
                {
                    LogFatalException(logger, firstTask.Exception!.GetBaseException());
                }

                faultOrCancellation.Cancel();
            }

            var exceptions = new List<Exception>();

            foreach (var consumeTask in consumeTasks)
            {
                try
                {
                    await consumeTask.ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (exception is not OperationCanceledException cancelException ||
                        cancelException.CancellationToken != faultOrCancellation.Token)
                    {
                        exceptions.Add(exception);
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            throw new OperationCanceledException("Hosted consumer stopped successfully.", stoppingToken);
        }
    }

#pragma warning restore CA1031

    async Task ConsumeLoop(IHostedConsumer consumer, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                await consumer.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (KafkaException exception) when (exception.Error is null || !exception.Error.IsFatal)
            {
                if (logger is not null && logger.IsEnabled(LogLevel.Warning))
                {
                    var code = exception.Error?.Code ?? ErrorCode.NoError;
                    LogKafkaError(logger, (int)code, exception);
                }
            }
        }
    }
}
