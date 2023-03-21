namespace Confluent.Kafka.Options;

sealed class ClientHandlers
{
    public Action<IClient, Error>? ErrorHandler { get; set; }

    public Action<IClient, string>? StatisticsHandler { get; set; }

    public Action<IClient, string>? AuthenticateHandler { get; set; }

    public Func<IClient, RebalancedOffsets, IEnumerable<TopicPartitionOffset>>? RebalanceHandler { get; set; }

    public Action<IClient, CommittedOffsets>? CommitHandler { get; set; }
}
