namespace Confluent.Kafka.Hosting;

/// <summary>
/// Options for processing multiple Kafka messages/events concurrently.
/// </summary>
public class ParallelConsumeOptions
{
    /// <summary>
    /// Gets or sets the maximum number of messages/events to process concurrently.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Environment.ProcessorCount"/>. Set to <c>0</c> to disable message processing.
    /// </remarks>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
}
