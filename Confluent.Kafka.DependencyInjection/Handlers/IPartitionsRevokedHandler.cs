using System;
using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Handlers
{
    /// <summary>
    /// A DI-friendly contract for handling Kafka revocations.
    /// </summary>
    /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetPartitionsRevokedHandler(Func{IConsumer{TKey, TValue}, List{TopicPartitionOffset}, IEnumerable{TopicPartitionOffset}})"/>
    public interface IPartitionsRevokedHandler
    {
        /// <summary>
        /// Handles (and optionally overrides) an automatic partition revocation.
        /// </summary>
        /// <remarks>
        /// To keep the default revocation, return an empty <c>Enumerable</c>.
        /// </remarks>
        /// <param name="consumer">The Kafka consumer.</param>
        /// <param name="offsets">The current partitions/offsets.</param>
        /// <returns>The partitions/offsets to use.</returns>
        IEnumerable<TopicPartitionOffset> OnPartitionsRevoked(IClient consumer, IEnumerable<TopicPartitionOffset> offsets);
    }
}
