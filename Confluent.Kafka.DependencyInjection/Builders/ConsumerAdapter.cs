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
    readonly IServiceProvider services;
    readonly IDisposable? scope;

    public ConsumerAdapter(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values)
    {
    }

    internal ConsumerAdapter(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>> config)
        : base(config)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();
        this.services = scope.ServiceProvider;
    }

    internal ConsumerAdapter(IServiceProvider services, IEnumerable<KeyValuePair<string, string>> config)
        : base(config)
    {
        this.services = services;
    }

    public override IConsumer<TKey, TValue> Build()
    {
        ErrorHandler ??= services.GetServices<IErrorHandler>()
            .Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        StatisticsHandler ??= services.GetServices<IStatisticsHandler>()
            .Aggregate(default(Action<IClient, string>), (x, y) => x + y.OnStatistics);

        LogHandler ??= services.GetServices<ILogHandler>()
            .Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        PartitionsAssignedHandler ??= services.GetServices<IPartitionsAssignedHandler>()
            .Aggregate(
                default(Func<IClient, IEnumerable<TopicPartition>, IEnumerable<TopicPartitionOffset>>),
                (x, y) => x + y.OnPartitionsAssigned);

        PartitionsRevokedHandler ??= services.GetServices<IPartitionsRevokedHandler>()
            .Aggregate(
                default(Func<IClient, IEnumerable<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>),
                (x, y) => x + y.OnPartitionsRevoked);

        OffsetsCommittedHandler ??= services.GetServices<IOffsetsCommittedHandler>()
            .Aggregate(default(Action<IClient, CommittedOffsets>), (x, y) => x + y.OnOffsetsCommitted);

        KeyDeserializer ??= services.GetService<IDeserializer<TKey>>() ??
            services.GetService<IAsyncDeserializer<TKey>>()?.AsSyncOverAsync();

        ValueDeserializer ??= services.GetService<IDeserializer<TValue>>() ??
            services.GetService<IAsyncDeserializer<TValue>>()?.AsSyncOverAsync();

        var consumer = base.Build();
        return scope != null ? new ScopedConsumer<TKey, TValue>(consumer, scope) : consumer;
    }
}
