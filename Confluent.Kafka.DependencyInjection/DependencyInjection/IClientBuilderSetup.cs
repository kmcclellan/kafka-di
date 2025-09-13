namespace Confluent.Kafka.DependencyInjection;

/// <summary>
/// Incrementally configures various Kafka client builders.
/// </summary>
public interface IClientBuilderSetup
{
    /// <summary>
    /// Applies this setup to a producer builder, if applicable.
    /// </summary>
    /// <typeparam name="TKey">The producer key type.</typeparam>
    /// <typeparam name="TValue">The producer value type.</typeparam>
    /// <param name="builder">The producer builder.</param>
    void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder);

    /// <summary>
    /// Applies this setup to a consumer builder, if applicable.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <param name="builder">The consumer builder.</param>
    void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder);

    /// <summary>
    /// Applies this setup to an admin client builder, if applicable.
    /// </summary>
    /// <param name="builder">The admin client builder.</param>
    void Apply(AdminClientBuilder builder);
}
