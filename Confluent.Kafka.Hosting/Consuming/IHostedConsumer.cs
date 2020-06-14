using System.Threading;
using System.Threading.Tasks;

namespace Confluent.Kafka.Hosting.Consuming
{
    /// <summary>
    /// Contract for a hosted consumer of Kafka messages.
    /// </summary>
    /// <remarks>
    /// Not to be confused with <see cref="IConsumer{TKey, TValue}"/>, which is involved in reading messages off the network.
    /// "Consuming" here implies the complete processing of the message (an arguably less idiosyncratic usage).
    /// </remarks>
    /// <typeparam name="TKey">The message key type.</typeparam>
    /// <typeparam name="TValue">The message value type.</typeparam>
    public interface IHostedConsumer<TKey, TValue>
    {
        /// <summary>
        /// Consume a message from Kafka.
        /// </summary>
        /// <param name="message">The Kafka message.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConsumeAsync(Message<TKey, TValue> message, CancellationToken cancellationToken);
    }
}
