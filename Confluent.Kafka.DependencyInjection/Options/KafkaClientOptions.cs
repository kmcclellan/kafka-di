namespace Confluent.Kafka.Options;

/// <summary>
/// Options for Kafka clients (producers, consumers, etc.).
/// </summary>
public class KafkaClientOptions
{
    readonly List<IClientConfigProvider> config = new();
    readonly List<IClientBuilderSetup> setups = new();

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
