using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection
{
    /// <summary>
    /// A DI-friendly contract for configuring an <see cref="IClient"/> (producer/consumer).
    /// </summary>
    /// <typeparam name="TClient">The type of the client.</typeparam>
    /// <typeparam name="TDescriptor">The return type for chaining.</typeparam>
    public interface IClientDescriptor<TClient, TDescriptor> where TClient : IClient where TDescriptor : IClientDescriptor<TClient, TDescriptor>
    {
        /// <summary>
        /// Sets client configuration values.
        /// </summary>
        /// <remarks>
        /// See <see href="https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see> for format.
        /// </remarks>
        /// <seealso cref="ConsumerConfig"/>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.ConsumerBuilder(IEnumerable{KeyValuePair{string, string}})"/>
        /// <param name="configuration"></param>
        /// <returns>The same instance for chaining.</returns>
        TDescriptor AddConfiguration(IEnumerable<KeyValuePair<string, string>> configuration);

        /// <summary>
        /// Enables standard Kafka logging through <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>.
        /// </summary>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetLogHandler(System.Action{IConsumer{TKey, TValue}, LogMessage})"/>
        /// <seealso cref="ProducerBuilder{TKey, TValue}.SetLogHandler(System.Action{IProducer{TKey, TValue}, LogMessage})"/>
        /// <returns>The same instance for chaining.</returns>
        TDescriptor AddLogging();

        /// <summary>
        /// Registers a Kafka statistics handler.
        /// </summary>
        /// <typeparam name="T">The handler type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        TDescriptor AddStatisticsHandler<T>() where T : class, IStatisticsHandler<TClient>;

        /// <summary>
        /// Registers a Kafka error handler. 
        /// </summary>
        /// <typeparam name="T">The handler type.</typeparam>
        /// <returns>The same instance for chaining.</returns>
        TDescriptor AddErrorHandler<T>() where T : class, IErrorHandler<TClient>;
    }

    /// <summary>
    /// A DI-friendly contract for Kafka statistics handling.
    /// </summary>
    /// <typeparam name="TClient">The type of the Kafka producer or consumer.</typeparam>
    public interface IStatisticsHandler<in TClient> where TClient : IClient
    {
        /// <summary>
        /// Handles periodic Kafka statistics.
        /// </summary>
        /// <remarks>
        /// Statistics are enabled by configuring <c>StatisticsIntervalMs</c> for the client.
        /// Refer to <see href="https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md">librdkafka documentation</see> for format.
        /// </remarks>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetStatisticsHandler(System.Action{IConsumer{TKey, TValue}, string})"/>
        /// <seealso cref="ProducerBuilder{TKey, TValue}.SetStatisticsHandler(System.Action{IProducer{TKey, TValue}, string})"/>
        /// <param name="client">The Kafka producer or consumer.</param>
        /// <param name="statistics">The JSON statistics data.</param>
        void OnStatistics(TClient client, string statistics);
    }

    /// <summary>
    /// A DI-friendly contract for Kafka error handling.
    /// </summary>
    /// <typeparam name="TClient">The type of the Kafka producer or consumer.</typeparam>
    public interface IErrorHandler<in TClient> where TClient : IClient
    {
        /// <summary>
        /// Handles an error triggered by asynchronous Kafka actions.
        /// </summary>
        /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetErrorHandler(System.Action{IConsumer{TKey, TValue}, Error})"/>
        /// <seealso cref="ProducerBuilder{TKey, TValue}.SetErrorHandler(System.Action{IProducer{TKey, TValue}, Error})"/>
        /// <param name="client">The Kafka producer or consumer.</param>
        /// <param name="error">The Kafka error.</param>
        void OnError(TClient client, Error error);
    }
}
