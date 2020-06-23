namespace Confluent.Kafka.DependencyInjection
{
    /// <summary>
    /// A contract for creating configured Kafka producers and consumers.
    /// </summary>
    public interface IKafkaFactory
    {
        /// <summary>
        /// Creates a configured producer.
        /// </summary>
        /// <typeparam name="TKey">The producer key type.</typeparam>
        /// <typeparam name="TValue">The producer value type.</typeparam>
        /// <returns>The producer.</returns>
        IProducer<TKey, TValue> CreateProducer<TKey, TValue>();

        /// <summary>
        /// Creates a configured consumer.
        /// </summary>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        /// <returns>The consumer.</returns>
        IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>();
    }
}
