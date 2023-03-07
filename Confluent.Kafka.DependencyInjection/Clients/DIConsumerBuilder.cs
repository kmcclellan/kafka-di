namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.SyncOverAsync;

sealed class DIConsumerBuilder<TKey, TValue> : ConsumerBuilder<TKey, TValue>
{
    readonly IEnumerable<IErrorHandler> errorHandlers;
    readonly IEnumerable<IStatisticsHandler> statisticsHandlers;
    readonly IEnumerable<ILogHandler> logHandlers;
    readonly IEnumerable<IPartitionsAssignedHandler> assignHandlers;
    readonly IEnumerable<IPartitionsRevokedHandler> revokeHandlers;
    readonly IEnumerable<IOffsetsCommittedHandler> commitHandlers;
    readonly IDeserializer<TKey>? keyDeserializer;
    readonly IDeserializer<TValue>? valueDeserializer;
    readonly IAsyncDeserializer<TKey>? asyncKeyDeserializer;
    readonly IAsyncDeserializer<TValue>? asyncValueDeserializer;

    public DIConsumerBuilder(
        ConsumerConfig config,
        IEnumerable<IErrorHandler> errorHandlers,
        IEnumerable<IStatisticsHandler> statisticsHandlers,
        IEnumerable<ILogHandler> logHandlers,
        IEnumerable<IPartitionsAssignedHandler> assignHandlers,
        IEnumerable<IPartitionsRevokedHandler> revokeHandlers,
        IEnumerable<IOffsetsCommittedHandler> commitHandlers,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null,
        IAsyncDeserializer<TKey>? asyncKeyDeserializer = null,
        IAsyncDeserializer<TValue>? asyncValueDeserializer = null)
        : base(config)
    {
        this.errorHandlers = errorHandlers;
        this.statisticsHandlers = statisticsHandlers;
        this.logHandlers = logHandlers;
        this.assignHandlers = assignHandlers;
        this.revokeHandlers = revokeHandlers;
        this.commitHandlers = commitHandlers;
        this.keyDeserializer = keyDeserializer;
        this.valueDeserializer = valueDeserializer;
        this.asyncKeyDeserializer = asyncKeyDeserializer;
        this.asyncValueDeserializer = asyncValueDeserializer;
    }

    public override IConsumer<TKey, TValue> Build()
    {
        this.ErrorHandler ??= this.errorHandlers.Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        this.StatisticsHandler ??= this.statisticsHandlers.Aggregate(
            default(Action<IClient, string>),
            (x, y) => x + y.OnStatistics);

        this.LogHandler ??= this.logHandlers.Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        this.PartitionsAssignedHandler ??= this.assignHandlers.Aggregate(
            default(Func<IClient, IEnumerable<TopicPartition>, IEnumerable<TopicPartitionOffset>>),
            (x, y) => x + y.OnPartitionsAssigned);

        this.PartitionsRevokedHandler ??= this.revokeHandlers.Aggregate(
            default(Func<IClient, IEnumerable<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>),
            (x, y) => x + y.OnPartitionsRevoked);

        this.OffsetsCommittedHandler ??= this.commitHandlers.Aggregate(
            default(Action<IClient, CommittedOffsets>),
            (x, y) => x + y.OnOffsetsCommitted);

        this.KeyDeserializer ??= this.keyDeserializer ?? this.asyncKeyDeserializer?.AsSyncOverAsync();
        this.ValueDeserializer ??= this.valueDeserializer ?? this.asyncValueDeserializer?.AsSyncOverAsync();

        return base.Build();
    }
}
