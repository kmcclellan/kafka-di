namespace Confluent.Kafka.DependencyInjection.Handlers.Default;

using Confluent.Kafka.DependencyInjection.Logging;

using Microsoft.Extensions.Logging;

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
                this.logger.Log(
                    group.Key.IsFatal ? LogLevel.Critical : LogLevel.Error,
                    default,
                    new KafkaLogValues(
                        client.Name,
                        $"Commit failed for offsets: {group.Key.Reason}",
                        group.ToList()),
                    new KafkaException(group.Key),
                    (x, _) => x.ToString());
            }
            else
            {
                this.logger.LogKafkaCommit(client, group);
            }
        }
    }
}
