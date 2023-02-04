namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.SyncOverAsync;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

sealed class ConsumerAdapter<TKey, TValue> : ConsumerBuilder<TKey, TValue>
{
    readonly IDisposable? scope;

    public ConsumerAdapter(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(config.Values, scopes.CreateScope(), dispose: true)
    {
    }

    internal ConsumerAdapter(
        IEnumerable<KeyValuePair<string, string>> config,
        IServiceScope scope,
        bool dispose)
        : base(config)
    {
        ErrorHandler = scope.ServiceProvider.GetServices<IErrorHandler>()
            .Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        StatisticsHandler = scope.ServiceProvider.GetServices<IStatisticsHandler>()
            .Aggregate(default(Action<IClient, string>), (x, y) => x + y.OnStatistics);

        LogHandler = scope.ServiceProvider.GetServices<ILogHandler>()
            .Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        PartitionsAssignedHandler = scope.ServiceProvider.GetServices<IPartitionsAssignedHandler>()
            .Aggregate(
                default(Func<IClient, IEnumerable<TopicPartition>, IEnumerable<TopicPartitionOffset>>),
                (x, y) => x + y.OnPartitionsAssigned);

        PartitionsRevokedHandler = scope.ServiceProvider.GetServices<IPartitionsRevokedHandler>()
            .Aggregate(
                default(Func<IClient, IEnumerable<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>),
                (x, y) => x + y.OnPartitionsRevoked);

        OffsetsCommittedHandler = scope.ServiceProvider.GetServices<IOffsetsCommittedHandler>()
            .Aggregate(default(Action<IClient, CommittedOffsets>), (x, y) => x + y.OnOffsetsCommitted);

        KeyDeserializer = scope.ServiceProvider.GetService<IDeserializer<TKey>>() ??
            scope.ServiceProvider.GetService<IAsyncDeserializer<TKey>>()?.AsSyncOverAsync();

        ValueDeserializer = scope.ServiceProvider.GetService<IDeserializer<TValue>>() ??
            scope.ServiceProvider.GetService<IAsyncDeserializer<TValue>>()?.AsSyncOverAsync();

        if (dispose)
        {
            this.scope = scope;
        }
    }

    public override IConsumer<TKey, TValue> Build()
    {
        var consumer = base.Build();
        return scope != null ? new ServiceConsumer<TKey, TValue>(consumer, scope) : consumer;
    }
}
