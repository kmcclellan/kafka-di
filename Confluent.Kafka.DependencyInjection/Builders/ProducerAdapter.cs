namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

sealed class ProducerAdapter<TKey, TValue> : ProducerBuilder<TKey, TValue>
{
    readonly IDisposable? scope;

    public ProducerAdapter(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(config.Values, scopes.CreateScope(), dispose: true)
    {
    }

    internal ProducerAdapter(
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

        KeySerializer = scope.ServiceProvider.GetService<ISerializer<TKey>>();
        ValueSerializer = scope.ServiceProvider.GetService<ISerializer<TValue>>();

        // Setting both types of serializers is an error.
        if (KeySerializer == null)
        {
            AsyncKeySerializer = scope.ServiceProvider.GetService<IAsyncSerializer<TKey>>();
        }

        if (ValueSerializer == null)
        {
            AsyncValueSerializer =scope.ServiceProvider.GetService<IAsyncSerializer<TValue>>();
        }

        if (dispose)
        {
            this.scope = scope;
        }
    }

    public override IProducer<TKey, TValue> Build()
    {
        var producer = base.Build();
        return scope != null ? new ServiceProducer<TKey, TValue>(producer, scope) : producer;
    }
}
