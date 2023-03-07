namespace Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A contract for creating configured Kafka producers and consumers.
/// </summary>
public interface IKafkaFactory
{
    /// <summary>
    /// Creates a configured producer.
    /// </summary>
    /// <remarks>
    /// Configuration will be merged with <see cref="ServiceCollectionExtensions.AddKafkaClient(IServiceCollection, IEnumerable{KeyValuePair{string, string}}?)"/>, if applicable.
    /// </remarks>
    /// <typeparam name="TKey">The producer key type.</typeparam>
    /// <typeparam name="TValue">The producer value type.</typeparam>
    /// <param name="configuration">Client configuration properties.</param>
    /// <returns>The producer.</returns>
    IProducer<TKey, TValue> CreateProducer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null);

    /// <summary>
    /// Creates a configured consumer.
    /// </summary>
    /// <remarks>
    /// Configuration will be merged with <see cref="ServiceCollectionExtensions.AddKafkaClient(IServiceCollection, IEnumerable{KeyValuePair{string, string}}?)"/>, if applicable.
    /// </remarks>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    /// <param name="configuration">Client configuration properties.</param>
    /// <returns>The consumer.</returns>
    IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null);
}
