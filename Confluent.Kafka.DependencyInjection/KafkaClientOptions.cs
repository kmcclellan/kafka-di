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
    /// Gets or sets the delegate to handle Kafka client errors.
    /// </summary>
    public Action<IClient, Error>? ErrorHandler { get; set; }

    /// <summary>
    /// Gets or sets the delegate to handle Kafka client statistics.
    /// </summary>
    /// <remarks>Statistics are JSON-serialized and described in <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md">librdkafka documentation</see>.</remarks>
    public Action<IClient, string>? StatisticsHandler { get; set; }

    /// <summary>
    /// Gets or sets the delegate to handle periodic Kafka client authentication.
    /// </summary>
    /// <remarks>
    /// For SASL/OAUTHBEARER authentication, handler is passed value of <c>sasl.oauthbearer.config</c> and should invoke <see cref="ClientExtensions.OAuthBearerSetToken(IClient, string, long, string, IDictionary{string, string})"/> or <see cref="ClientExtensions.OAuthBearerSetTokenFailure(IClient, string)"/>.
    /// </remarks>
    public Action<IClient, string>? AuthenticateHandler { get; set; }

    /// <summary>
    /// Gets or sets the delegate to handle and/or override offsets assigned, revoked, or lost as part of a consumer group rebalance.
    /// </summary>
    /// <remarks>
    /// The consumer will continue using the returned offsets (none to accept revocation/loss).
    /// </remarks>
    public Func<IClient, RebalancedOffsets, IEnumerable<TopicPartitionOffset>>? RebalanceHandler { get; set; }

    /// <summary>
    /// Gets or sets the delegate to handle offsets and/or errors from automatic consumer commits (<c>enable.auto.commit</c>).
    /// </summary>
    public Action<IClient, CommittedOffsets>? CommitHandler { get; set; }

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

    /// <summary>
    /// Configures producers to use the specified partitioner.
    /// </summary>
    /// <param name="partitioner">The partitioner delegate.</param>
    /// <param name="topic">The topic to which this partitioner applies, or <see langword="null"/> for all.</param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions Use(PartitionerDelegate partitioner, string? topic = null)
    {
        this.Setups.Add(new PartitionerSetup(partitioner, topic));
        return this;
    }
}
