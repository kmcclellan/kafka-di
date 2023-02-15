namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;

sealed class ProducerAdapter<TKey, TValue> : ProducerBuilder<TKey, TValue>
{
    readonly IEnumerable<IErrorHandler> errorHandlers;
    readonly IEnumerable<IStatisticsHandler> statisticsHandlers;
    readonly IEnumerable<ILogHandler> logHandlers;
    readonly ISerializer<TKey>? keySerializer;
    readonly ISerializer<TValue>? valueSerializer;
    readonly IAsyncSerializer<TKey>? asyncKeySerializer;
    readonly IAsyncSerializer<TValue>? asyncValueSerializer;

    public ProducerAdapter(
        ConfigWrapper config,
        IEnumerable<IErrorHandler> errorHandlers,
        IEnumerable<IStatisticsHandler> statisticsHandlers,
        IEnumerable<ILogHandler> logHandlers,
        ISerializer<TKey>? keySerializer = null,
        ISerializer<TValue>? valueSerializer = null,
        IAsyncSerializer<TKey>? asyncKeySerializer = null,
        IAsyncSerializer<TValue>? asyncValueSerializer = null)
        : base(config.Values)
    {
        this.errorHandlers = errorHandlers;
        this.statisticsHandlers = statisticsHandlers;
        this.logHandlers = logHandlers;
        this.keySerializer = keySerializer;
        this.valueSerializer = valueSerializer;
        this.asyncKeySerializer = asyncKeySerializer;
        this.asyncValueSerializer = asyncValueSerializer;
    }

    public override IProducer<TKey, TValue> Build()
    {
        ErrorHandler ??= errorHandlers.Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        StatisticsHandler ??= statisticsHandlers.Aggregate(
            default(Action<IClient, string>),
            (x, y) => x + y.OnStatistics);

        LogHandler ??= logHandlers.Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        // Setting both types of serializers is an error.
        if ((KeySerializer ??= keySerializer) == null)
        {
            AsyncKeySerializer ??= asyncKeySerializer;
        }

        if ((ValueSerializer ??= valueSerializer) == null)
        {
            AsyncValueSerializer ??= asyncValueSerializer;
        }

        return base.Build();
    }
}
