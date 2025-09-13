namespace Confluent.Kafka.DependencyInjection;

/// <summary>
/// A provider of config properties for Kafka clients.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see>.
/// </remarks>
public interface IClientConfigProvider
{
    /// <summary>
    /// Gets Kafka config properties to be used by producers.
    /// </summary>
    /// <typeparam name="TKey">The producer key type.</typeparam>
    /// <typeparam name="TValue">The producer value type.</typeparam>
    /// <returns>The producer properties.</returns>
    IEnumerator<KeyValuePair<string, string>> ForProducer<TKey, TValue>();

    /// <summary>
    /// Gets Kafka config properties to be used by consumers.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <returns>The consumer properties.</returns>
    IEnumerator<KeyValuePair<string, string>> ForConsumer<TKey, TValue>();

    /// <summary>
    /// Gets Kafka config properties to be used by admin clients.
    /// </summary>
    /// <returns>The admin client properties.</returns>
    IEnumerator<KeyValuePair<string, string>> ForAdminClient();
}
