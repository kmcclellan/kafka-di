namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Handlers;

sealed class DIProducerBuilder<TKey, TValue> : ProducerBuilder<TKey, TValue>
{
    readonly IEnumerable<IErrorHandler> errorHandlers;
    readonly IEnumerable<IStatisticsHandler> statisticsHandlers;
    readonly IEnumerable<ILogHandler> logHandlers;
    readonly ISerializer<TKey>? keySerializer;
    readonly ISerializer<TValue>? valueSerializer;
    readonly IAsyncSerializer<TKey>? asyncKeySerializer;
    readonly IAsyncSerializer<TValue>? asyncValueSerializer;

    public DIProducerBuilder(
        ProducerConfig config,
        IEnumerable<IErrorHandler> errorHandlers,
        IEnumerable<IStatisticsHandler> statisticsHandlers,
        IEnumerable<ILogHandler> logHandlers,
        ISerializer<TKey>? keySerializer = null,
        ISerializer<TValue>? valueSerializer = null,
        IAsyncSerializer<TKey>? asyncKeySerializer = null,
        IAsyncSerializer<TValue>? asyncValueSerializer = null)
        : base(config)
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
        this.ErrorHandler ??= this.errorHandlers.Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        this.StatisticsHandler ??= this.statisticsHandlers.Aggregate(
            default(Action<IClient, string>),
            (x, y) => x + y.OnStatistics);

        this.LogHandler ??= this.logHandlers.Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        // Setting both types of serializers is an error.
        if ((this.KeySerializer ??= this.keySerializer) == null)
        {
            this.AsyncKeySerializer ??= this.asyncKeySerializer;
        }

        if ((this.ValueSerializer ??= this.valueSerializer) == null)
        {
            this.AsyncValueSerializer ??= this.asyncValueSerializer;
        }

        return base.Build();
    }
}
