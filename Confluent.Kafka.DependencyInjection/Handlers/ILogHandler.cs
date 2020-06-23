using System;

namespace Confluent.Kafka.DependencyInjection.Handlers
{
    /// <summary>
    /// A DI-friendly contract for handling Kafka log messages.
    /// </summary>
    /// <seealso cref="ProducerBuilder{TKey, TValue}.SetLogHandler(Action{IProducer{TKey, TValue}, LogMessage})"/>
    /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetLogHandler(Action{IConsumer{TKey, TValue}, LogMessage})"/>
    public interface ILogHandler
    {
        /// <summary>
        /// Handles a log message from a producer or consumer.
        /// </summary>
        /// <param name="client">The Kafka producer or consumer.</param>
        /// <param name="message">The Kafka log message.</param>
        void OnLog(IClient client, LogMessage message);
    }
}
