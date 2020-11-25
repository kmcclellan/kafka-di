using System.Linq;
using Confluent.Kafka.DependencyInjection.Logging;
using Microsoft.Extensions.Logging;

namespace Confluent.Kafka.DependencyInjection.Handlers.Default
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class CommitHandler : IOffsetsCommittedHandler
    {
        readonly ILogger<CommitHandler> logger;

        public CommitHandler(ILogger<CommitHandler> logger)
        {
            this.logger = logger;
        }

        public void OnOffsetsCommitted(IClient client, CommittedOffsets offsets)
        {
            foreach (var group in offsets.Offsets.GroupBy(
                o => o.Error.IsError ? o.Error : offsets.Error,
                o => o.TopicPartitionOffset))
            {
                if (group.Key.IsError)
                {
                    logger.LogKafkaOffsets(
                        client,
                        group.Key,
                        $"Commit failed for offsets: {group.Key.Reason}",
                        group);
                }
                else
                {
                    logger.LogKafkaOffsets(
                        client,
                        LogEvents.OffsetsCommitted,
                        "Offsets committed",
                        group);
                }
            }
        }
    }
}
