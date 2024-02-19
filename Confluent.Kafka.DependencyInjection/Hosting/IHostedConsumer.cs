namespace Confluent.Kafka.Hosting;

/// <summary>
/// A continuous consumer and processor of Kafka messages and events.
/// </summary>
public interface IHostedConsumer
{
    /// <summary>
    /// Consumes Kafka messages indefinitely.
    /// </summary>
    /// <param name="cancellationToken">A token for cancellation.</param>
    /// <returns>A task for the consume operation (will only complete if faulted/cancelled).</returns>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="ConsumeException"/>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
