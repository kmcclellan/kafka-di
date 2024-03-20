namespace Confluent.Kafka.Hosting;

/// <summary>
/// A processor of consumed Kafka messages and events.
/// </summary>
/// <typeparam name="TKey">The consumer key type.</typeparam>
/// <typeparam name="TValue">The consumer value type.</typeparam>
public interface IConsumerProcessor<TKey, TValue>
{
    /// <summary>
    /// Processes a Kafka consume result asynchronously.
    /// </summary>
    /// <param name="result">A result containing the consumed message/event.</param>
    /// <param name="cancellationToken">A token for cancellation.</param>
    /// <returns>A task for the process operation.</returns>
    ValueTask ProcessAsync(ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken);
}
