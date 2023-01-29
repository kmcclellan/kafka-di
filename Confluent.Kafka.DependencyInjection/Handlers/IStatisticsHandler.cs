namespace Confluent.Kafka.DependencyInjection.Handlers;

using System;

/// <summary>
/// A DI-friendly contract for Kafka statistics handling.
/// </summary>
/// <seealso cref="ProducerBuilder{TKey, TValue}.SetStatisticsHandler(Action{IProducer{TKey, TValue}, string})"/>
/// <seealso cref="ConsumerBuilder{TKey, TValue}.SetStatisticsHandler(Action{IConsumer{TKey, TValue}, string})"/>
public interface IStatisticsHandler
{
    /// <summary>
    /// Handles periodic Kafka statistics.
    /// </summary>
    /// <remarks>
    /// Statistics are enabled by configuring <c>StatisticsIntervalMs</c> for the client.
    /// Refer to <see href="https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md">librdkafka documentation</see> for format.
    /// </remarks>
    /// <param name="client">The Kafka producer or consumer.</param>
    /// <param name="statistics">The JSON statistics data.</param>
    void OnStatistics(IClient client, string statistics);
}
