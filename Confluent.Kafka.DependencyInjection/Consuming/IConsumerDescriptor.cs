using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Consuming
{
    /// <summary>
    /// A DI-friendly contract for configuring an <see cref="IConsumer{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    public interface IConsumerDescriptor<TKey, TValue> : IClientDescriptor<IConsumer<TKey, TValue>, IConsumerDescriptor<TKey, TValue>>
    {
        /// <summary>
        /// Adds topics to the initial consumer subscription.
        /// </summary>
        /// <seealso cref="IConsumer{TKey, TValue}.Subscribe(IEnumerable{string})"/>
        /// <param name="topics">The topics.</param>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> AddTopics(IEnumerable<string> topics);

        /// <summary>
        /// Registers a deserializer to use for message keys.
        /// </summary>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetKeyDeserializer(IDeserializer{TKey})"/>
        /// <typeparam name="T">The deserializer type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> SetKeyDeserializer<T>() where T : class, IDeserializer<TKey>;

        /// <summary>
        /// Registers a deserializer to use for message values.
        /// </summary>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetValueDeserializer(IDeserializer{TValue})"/>
        /// <typeparam name="T">The deserializer type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> SetValueDeserializer<T>() where T : class, IDeserializer<TValue>;

        /// <summary>
        /// Registers a Kafka commit handler.
        /// </summary>
        /// <typeparam name="T">The handler type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> AddOffsetsCommittedHandler<T>() where T : class, IOffsetsCommittedHandler<TKey, TValue>;

        /// <summary>
        /// Registers a Kafka assignment handler.
        /// </summary>
        /// <typeparam name="T">The handler type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> SetPartitionsAssignedHandler<T>() where T : class, IPartitionsAssignedHandler<TKey, TValue>;

        /// <summary>
        /// Registers a Kafka revocation handler.
        /// </summary>
        /// <typeparam name="T">The handler type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        IConsumerDescriptor<TKey, TValue> SetPartitionsRevokedHandler<T>() where T : class, IPartitionsRevokedHandler<TKey, TValue>;
    }

    /// <summary>
    /// A DI-friendly contract for handling automatic Kafka commits.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    public interface IOffsetsCommittedHandler<TKey, TValue>
    {
        /// <summary>
        /// Handles a Kafka auto-commit.
        /// </summary>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetOffsetsCommittedHandler(System.Action{IConsumer{TKey, TValue}, CommittedOffsets})"/>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="offsets">The committed offsets.</param>
        void OnOffsetsCommitted(IConsumer<TKey, TValue> consumer, CommittedOffsets offsets);
    }

    /// <summary>
    /// A DI-friendly contract for handling Kafka assignments.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    public interface IPartitionsAssignedHandler<TKey, TValue>
    {
        /// <summary>
        /// Handles (and optionally overrides) a partition assignment.
        /// </summary>
        /// <remarks>
        /// To keep the default assignment, return the same list with <see cref="Offset.Unset"/>.
        /// </remarks>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetPartitionsAssignedHandler(System.Func{IConsumer{TKey, TValue}, List{TopicPartition}, IEnumerable{TopicPartitionOffset}})"/>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="partitions">The assigned partitions.</param>
        /// <returns>The partitions and start offsets for consuming.</returns>
        IEnumerable<TopicPartitionOffset> OnPartitionsAssigned(IConsumer<TKey, TValue> consumer, IEnumerable<TopicPartition> partitions);
    }

    /// <summary>
    /// A DI-friendly contract for handling Kafka revocations.
    /// </summary>
    /// <typeparam name="TKey">The consumer key type.</typeparam>
    /// <typeparam name="TValue">The consumer value type.</typeparam>
    public interface IPartitionsRevokedHandler<TKey, TValue>
    {
        /// <summary>
        /// Handles (and optionally overrides) a partition revocation.
        /// </summary>
        /// <remarks>
        /// To keep the default revocation, return an empty <c>Enumerable</c>.
        /// </remarks>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetPartitionsRevokedHandler(System.Func{IConsumer{TKey, TValue}, List{TopicPartitionOffset}, IEnumerable{TopicPartitionOffset}})"/>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="offsets">The current offsets</param>
        /// <returns>The partitions and start offsets for consuming.</returns>
        IEnumerable<TopicPartitionOffset> OnPartitionsRevoked(IConsumer<TKey, TValue> consumer, IEnumerable<TopicPartitionOffset> offsets);
    }
}
