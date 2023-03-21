namespace Confluent.Kafka.Options;

using Confluent.Kafka.SyncOverAsync;

/// <summary>
/// Options for Kafka clients (producers, consumers, etc.).
/// </summary>
public class KafkaClientOptions
{
    readonly List<IClientConfigProvider> config = new();
    readonly List<IClientBuilderSetup> setups = new();

    readonly Dictionary<string, string> producerConfig = new();
    readonly Dictionary<string, string> consumerConfig = new();
    readonly Dictionary<string, string> adminClientConfig = new();

    /// <summary>
    /// Initializes the options.
    /// </summary>
    public KafkaClientOptions()
    {
        this.config.Add(new StaticConfig(this.producerConfig, this.consumerConfig, this.adminClientConfig));
    }

    /// <summary>
    /// Configures client properties using a config provider.
    /// </summary>
    /// <param name="provider">The client config provider.</param>
    /// <returns>The same instance, for chaining.</returns>
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
    public KafkaClientOptions Deserialize<T>(IAsyncDeserializer<T> deserializer)
    {
        return this.Deserialize(deserializer.AsSyncOverAsync());
    }

    /// <summary>
    /// Creates a producer using the options.
    /// </summary>
    /// <typeparam name="TKey">The producer key type.</typeparam>
    /// <typeparam name="TValue">The producer value type.</typeparam>
    /// <returns>The producer.</returns>
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
