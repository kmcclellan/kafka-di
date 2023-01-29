namespace Confluent.Kafka.DependencyInjection.Handlers;

using System;
using System.Collections.Generic;

/// <summary>
/// A DI-friendly contract for handling Kafka assignments.
/// </summary>
/// <seealso cref="ConsumerBuilder{TKey, TValue}.SetPartitionsAssignedHandler(Func{IConsumer{TKey, TValue}, List{TopicPartition}, IEnumerable{TopicPartitionOffset}})"/>
public interface IPartitionsAssignedHandler
{
    /// <summary>
    /// Handles (and optionally overrides) an automatic partition assignment.
    /// </summary>
    /// <remarks>
    /// To keep the default assignment, return the same list with <see cref="Offset.Unset"/>.
    /// </remarks>
    /// <param name="consumer">The Kafka consumer.</param>
    /// <param name="partitions">The assigned partitions.</param>
    /// <returns>The partitions/offsets to use.</returns>
    IEnumerable<TopicPartitionOffset> OnPartitionsAssigned(IClient consumer, IEnumerable<TopicPartition> partitions);
}
