using System.Threading;
using System.Threading.Tasks;

namespace Confluent.Kafka.Hosting
{
    /// <summary>
    /// A contract for doing continuous work in a hosted setting.
    /// </summary>
    public interface IHostedWorker
    {
        /// <summary>
        /// Performs a unit of work.
        /// </summary>
        /// <param name="cancellationToken">A token for cancellation.</param>
        /// <returns>A task representing the work.</returns>
        Task Work(CancellationToken cancellationToken);
    }
}
