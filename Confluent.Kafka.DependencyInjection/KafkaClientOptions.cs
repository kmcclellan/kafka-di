namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Setups;
using Confluent.Kafka.SyncOverAsync;

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

    /// <summary>
    /// Configures producers to use the specified serializer.
    /// </summary>
    /// <typeparam name="T">The producer key/value type.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions Use<T>(ISerializer<T> serializer)
    {
        this.Setups.Add(new SerializerSetup<T>(serializer));
        return this;
    }

    /// <summary>
    /// Configures producers to use the specified asynchronous serializer.
    /// </summary>
    /// <typeparam name="T">The producer key/value type.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="nonblocking">
    /// Whether to prioritize thread resources over API flexibility.
    /// If <see langword="true"/>, only task-based producing is supported (e.g. <see cref="IProducer{TKey, TValue}.ProduceAsync(TopicPartition, Message{TKey, TValue}, CancellationToken)"/>).
    /// Must be set to <see langword="false"/> in order to use delivery callbacks (e.g. <see cref="IProducer{TKey, TValue}.Produce(TopicPartition, Message{TKey, TValue}, Action{DeliveryReport{TKey, TValue}})"/>).
    /// </param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions Use<T>(IAsyncSerializer<T> serializer, bool nonblocking = false)
    {
        if (nonblocking)
        {
            this.Setups.Add(new AsyncSerializerSetup<T>(serializer));
            return this;
        }
        else
        {
            return this.Use(serializer.AsSyncOverAsync());
        }
    }

    /// <summary>
    /// Configures consumers to use the specified deserializer.
    /// </summary>
    /// <typeparam name="T">The consumer key/value type.</typeparam>
    /// <param name="deserializer">The deserializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions Use<T>(IDeserializer<T> deserializer)
    {
        this.Setups.Add(new DeserializerSetup<T>(deserializer));
        return this;
    }

    /// <summary>
    /// Configures consumers to use the specified asynchronous deserializer.
    /// </summary>
    /// <typeparam name="T">The consumer key/value type.</typeparam>
    /// <param name="deserializer">The deserializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions Use<T>(IAsyncDeserializer<T> deserializer)
    {
        return this.Use(deserializer.AsSyncOverAsync());
    }
}
