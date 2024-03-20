namespace Confluent.Kafka.Hosting;

/// <summary>
/// Options for processing Kafka messages/events as a hosted service.
/// </summary>
public class HostedConsumeOptions : ParallelConsumeOptions
{
    /// <summary>
    /// Gets the collection of subscribed Kafka topics.
    /// </summary>
    public ISet<string> Topics { get; } = new HashSet<string>();
}
