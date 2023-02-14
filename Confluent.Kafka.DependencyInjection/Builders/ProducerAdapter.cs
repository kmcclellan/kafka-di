namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

sealed class ProducerAdapter<TKey, TValue> : ProducerBuilder<TKey, TValue>
{
    readonly IServiceProvider services;
    readonly IDisposable? scope;

    public ProducerAdapter(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values)
    {
    }

    internal ProducerAdapter(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>> config)
        : base(config)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();
        this.services = scope.ServiceProvider;
    }

    internal ProducerAdapter(IServiceProvider services, IEnumerable<KeyValuePair<string, string>> config)
        : base(config)
    {
        this.services = services;
    }

    public override IProducer<TKey, TValue> Build()
    {
        ErrorHandler ??= services.GetServices<IErrorHandler>()
            .Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        StatisticsHandler ??= services.GetServices<IStatisticsHandler>()
            .Aggregate(default(Action<IClient, string>), (x, y) => x + y.OnStatistics);

        LogHandler ??= services.GetServices<ILogHandler>()
            .Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        // Setting both types of serializers is an error.
        if ((KeySerializer ??= services.GetService<ISerializer<TKey>>()) == null)
        {
            AsyncKeySerializer ??= services.GetService<IAsyncSerializer<TKey>>();
        }

        if ((ValueSerializer ??= services.GetService<ISerializer<TValue>>()) == null)
        {
            AsyncValueSerializer ??= services.GetService<IAsyncSerializer<TValue>>();
        }

        var producer = base.Build();
        return scope != null ? new ScopedProducer<TKey, TValue>(producer, scope) : producer;
    }
}
