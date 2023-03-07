namespace Confluent.Kafka.DependencyInjection;

/// <summary>
/// Options for Kafka clients (producers, consumers, etc.).
/// </summary>
public class KafkaClientOptions
{
    /// <summary>
    /// Gets the dictionary of client configuration properties (see <see href="https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see>).
    /// </summary>
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the collection of "setups" for configuring client builders.
    /// </summary>
    public ICollection<IKafkaClientSetup> Setups { get; } = new List<IKafkaClientSetup>();
}
