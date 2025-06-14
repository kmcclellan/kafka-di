namespace Confluent.Kafka.Options;

using Confluent.Kafka.SyncOverAsync;

/// <summary>
/// Options for Kafka clients (producers, consumers, etc.).
/// </summary>
[Obsolete(DeprecationMessage)]
public class KafkaClientOptions
{
    internal const string DeprecationMessage = "Configure/resolve clients through the service container directly. " +
        "This type will be removed in a future version.";

    static readonly ClientConfig DefaultConfig = new() { BootstrapServers = "localhost:9092" };

    readonly List<IClientConfigProvider> config = [];
    readonly List<IClientBuilderSetup> setups = [];

    readonly Dictionary<string, string> producerConfig = [];
    readonly Dictionary<string, string> consumerConfig = [];
    readonly Dictionary<string, string> adminClientConfig = [];

    readonly ClientHandlers handlers = new();

    /// <summary>
    /// Initializes the options.
    /// </summary>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions()
    {
        this.config.Add(new StaticConfig(this.producerConfig, this.consumerConfig, this.adminClientConfig));
        this.setups.Add(new HandlerSetup(this.handlers));

        foreach (var kvp in DefaultConfig)
        {
            this.producerConfig.Add(kvp.Key, kvp.Value);
            this.consumerConfig.Add(kvp.Key, kvp.Value);
            this.adminClientConfig.Add(kvp.Key, kvp.Value);
        }
    }

    internal IEnumerable<IClientConfigProvider> ConfigProviders => config;

    internal IEnumerable<IClientBuilderSetup> BuilderSetups => setups;

    /// <summary>
    /// Configures client properties using a config provider.
    /// </summary>
    /// <param name="provider">The client config provider.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Configure(IClientConfigProvider provider)
    {
        this.config.Add(provider);
        return this;
    }

    /// <summary>
    /// Configures producer properties.
    /// </summary>
    /// <param name="config">The config properties.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Configure(ProducerConfig config)
    {
        foreach (var kvp in config ?? throw new ArgumentNullException(nameof(config)))
        {
            this.producerConfig[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Configures consumer properties.
    /// </summary>
    /// <param name="config">The config properties.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Configure(ConsumerConfig config)
    {
        foreach (var kvp in config ?? throw new ArgumentNullException(nameof(config)))
        {
            this.consumerConfig[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Configures admin client properties.
    /// </summary>
    /// <param name="config">The config properties.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Configure(AdminClientConfig config)
    {
        foreach (var kvp in config ?? throw new ArgumentNullException(nameof(config)))
        {
            this.adminClientConfig[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Configures clients using a builder setup.
    /// </summary>
    /// <param name="setup">The client builder setup.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Setup(IClientBuilderSetup setup)
    {
        this.setups.Add(setup);
        return this;
    }

    /// <summary>
    /// Configures producers to use the specified serializer.
    /// </summary>
    /// <typeparam name="T">The producer key/value type.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Serialize<T>(ISerializer<T> serializer)
    {
        this.setups.Add(new SerdesSetup<T>(serializer: serializer));
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
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Serialize<T>(IAsyncSerializer<T> serializer, bool nonblocking = false)
    {
        if (nonblocking)
        {
            this.setups.Add(new SerdesSetup<T>(asyncSerializer: serializer));
            return this;
        }
        else
        {
            return this.Serialize(serializer.AsSyncOverAsync());
        }
    }

    /// <summary>
    /// Configures consumers to use the specified deserializer.
    /// </summary>
    /// <typeparam name="T">The consumer key/value type.</typeparam>
    /// <param name="deserializer">The deserializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Deserialize<T>(IDeserializer<T> deserializer)
    {
        this.setups.Add(new SerdesSetup<T>(deserializer: deserializer));
        return this;
    }

    /// <summary>
    /// Configures consumers to use the specified asynchronous deserializer.
    /// </summary>
    /// <typeparam name="T">The consumer key/value type.</typeparam>
    /// <param name="deserializer">The deserializer.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Deserialize<T>(IAsyncDeserializer<T> deserializer)
    {
        return this.Deserialize(deserializer.AsSyncOverAsync());
    }

    /// <summary>
    /// Configures producers to use the specified partitioner.
    /// </summary>
    /// <param name="partitioner">The partitioner delegate.</param>
    /// <param name="topic">The topic to which this partitioner applies, or <see langword="null"/> for all.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions Partition(PartitionerDelegate partitioner, string? topic = null)
    {
        this.setups.Add(new PartitionerSetup(partitioner, topic));
        return this;
    }

    /// <summary>
    /// Configures clients to handle Kafka errors.
    /// </summary>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions OnError(Action<IClient, Error> handler)
    {
        this.handlers.ErrorHandler += handler;
        return this;
    }

    /// <summary>
    /// Configures clients to handle Kafka statistics.
    /// </summary>
    /// <remarks>Statistics are JSON-serialized and described in <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md">librdkafka documentation</see>.</remarks>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions OnStatistics(Action<IClient, string> handler)
    {
        this.handlers.StatisticsHandler += handler;
        return this;
    }

    /// <summary>
    /// Configures clients to handle periodic Kafka authentication.
    /// </summary>
    /// <remarks>
    /// For SASL/OAUTHBEARER authentication, handler is passed value of <c>sasl.oauthbearer.config</c> and should invoke <see cref="ClientExtensions.OAuthBearerSetToken(IClient, string, long, string, IDictionary{string, string})"/> or <see cref="ClientExtensions.OAuthBearerSetTokenFailure(IClient, string)"/>.
    /// </remarks>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions OnAuthenticate(Action<IClient, string> handler)
    {
        this.handlers.AuthenticateHandler += handler;
        return this;
    }

    /// <summary>
    /// Configures consumers to handle offsets assigned, revoked, or lost as part of a consumer group rebalance.
    /// </summary>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions OnRebalance(Action<IClient, RebalancedOffsets> handler)
    {
        return this.OnRebalance(
            (client, rebalance) =>
            {
                handler(client, rebalance);

                return rebalance.Revoked || rebalance.Lost
                    ? Enumerable.Empty<TopicPartitionOffset>()
                    : rebalance.Offsets;
            });
    }

    /// <summary>
    /// Configures consumers to override offsets assigned, revoked, or lost as part of a consumer group rebalance.
    /// </summary>
    /// <remarks>
    /// The consumer will continue using the returned offsets (none to accept revocation/loss).
    /// </remarks>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    [Obsolete(DeprecationMessage)]
    public KafkaClientOptions OnRebalance(Func<IClient, RebalancedOffsets, IEnumerable<TopicPartitionOffset>> handler)
    {
        this.handlers.RebalanceHandler += handler;
        return this;
    }

    /// <summary>
    /// Configures consumers handle offsets and/or errors from automatic consumer commits (<c>enable.auto.commit</c>).
    /// </summary>
    /// <param name="handler">The handler delegate.</param>
    /// <returns>The same instance, for chaining.</returns>
    public KafkaClientOptions OnCommit(Action<IClient, CommittedOffsets> handler)
    {
        this.handlers.CommitHandler += handler;
        return this;
    }

    /// <summary>
    /// Creates a producer using the options.
    /// </summary>
    /// <typeparam name="TKey">The producer key type.</typeparam>
    /// <typeparam name="TValue">The producer value type.</typeparam>
    /// <returns>The producer.</returns>
    [Obsolete(DeprecationMessage)]
    public IProducer<TKey, TValue> CreateProducer<TKey, TValue>()
    {
        var builder = new ProducerBuilder<TKey, TValue>(
            this.GetProperties(x => x.ForProducer<TKey, TValue>()));

        foreach (var setup in this.setups)
        {
            setup.Apply(builder);
        }

        return builder.Build();
    }

    /// <summary>
    /// Creates a consumer using the options.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <returns>The consumer.</returns>
    [Obsolete(DeprecationMessage)]
    public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>()
    {
        var builder = new ConsumerBuilder<TKey, TValue>(
            this.GetProperties(x => x.ForConsumer<TKey, TValue>()));

        foreach (var setup in this.setups)
        {
            setup.Apply(builder);
        }

        return builder.Build();
    }

    /// <summary>
    /// Creates an admin client using the options.
    /// </summary>
    /// <returns>The admin client.</returns>
    [Obsolete(DeprecationMessage)]
    public IAdminClient CreateAdminClient()
    {
        var builder = new AdminClientBuilder(
            this.GetProperties(x => x.ForAdminClient()));

        foreach (var setup in this.setups)
        {
            setup.Apply(builder);
        }

        return builder.Build();
    }

    IEnumerable<KeyValuePair<string, string>> GetProperties(
        Func<IClientConfigProvider, IEnumerator<KeyValuePair<string, string>>> invoke)
    {
        foreach (var provider in this.config)
        {
            using var props = invoke(provider);

            while (props.MoveNext())
            {
                yield return props.Current;
            }
        }
    }
}
