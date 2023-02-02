namespace Confluent.Kafka.DependencyInjection.Handlers.Default;

using Confluent.Kafka.DependencyInjection.Logging;

using Microsoft.Extensions.Logging;

using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class CommitHandler : IOffsetsCommittedHandler
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
                logger.Log(
                    group.Key.IsFatal ? LogLevel.Critical : LogLevel.Error,
                    LogEvents.FromError(group.Key.Code),
                    new KafkaLogValues(
                        client,
                        $"Commit failed for offsets: {group.Key.Reason}",
                        group.ToList()),
                    null,
                    (x, _) => x.ToString());
            }
            else
            {
                logger.LogKafkaCommit(client, group);
            }
        }
    }
}
