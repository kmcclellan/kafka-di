using System;

namespace Confluent.Kafka.DependencyInjection.Handlers
{
    /// <summary>
    /// A DI-friendly contract for Kafka error handling.
    /// </summary>
    /// <seealso cref="ProducerBuilder{TKey, TValue}.SetErrorHandler(Action{IProducer{TKey, TValue}, Error})"/>
    /// <seealso cref="ConsumerBuilder{TKey, TValue}.SetErrorHandler(Action{IConsumer{TKey, TValue}, Error})"/>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handles an error triggered by asynchronous Kafka actions.
        /// </summary>
        /// <param name="client">The Kafka producer or consumer.</param>
        /// <param name="err">The Kafka error.</param>
        void OnError(IClient client, Error err);
    }
}
