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
        if (offsets.Error.IsError)
        {
            this.logger.LogKafkaError(client, offsets.Error);
        }
        else
        {
            foreach (var group in offsets.Offsets.GroupBy(x => x.Error, x => x.TopicPartitionOffset))
            {
                if (group.Key.IsError)
                {
                    this.logger.LogKafkaError(client, group.Key);
                }
                else
                {
                    this.logger.LogKafkaCommit(client, group);
                }
            }

        }
    }
}
