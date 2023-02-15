namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.SyncOverAsync;

using System;
using System.Collections.Generic;
using System.Linq;

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
        ConfigWrapper config,
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
        : base(config.Values)
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
        ErrorHandler ??= errorHandlers.Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        StatisticsHandler ??= statisticsHandlers.Aggregate(
            default(Action<IClient, string>),
            (x, y) => x + y.OnStatistics);

        LogHandler ??= logHandlers.Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        PartitionsAssignedHandler ??= assignHandlers.Aggregate(
            default(Func<IClient, IEnumerable<TopicPartition>, IEnumerable<TopicPartitionOffset>>),
            (x, y) => x + y.OnPartitionsAssigned);

        PartitionsRevokedHandler ??= revokeHandlers.Aggregate(
            default(Func<IClient, IEnumerable<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>),
            (x, y) => x + y.OnPartitionsRevoked);

        OffsetsCommittedHandler ??= commitHandlers.Aggregate(
            default(Action<IClient, CommittedOffsets>),
            (x, y) => x + y.OnOffsetsCommitted);

        KeyDeserializer ??= keyDeserializer ?? asyncKeyDeserializer?.AsSyncOverAsync();
        ValueDeserializer ??=  valueDeserializer ?? asyncValueDeserializer?.AsSyncOverAsync();

        return base.Build();
    }
}
