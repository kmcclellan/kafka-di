namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Handlers;

using System.Collections.Generic;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ProducerAdapter<TKey, TValue> : ProducerBuilder<TKey, TValue>, IBuilderAdapter<IProducer<TKey, TValue>>
{
    public IDictionary<string, string> ClientConfig { get; } = new Dictionary<string, string>();

    public ProducerAdapter(
        HandlerHelper<IErrorHandler> errorHelper,
        HandlerHelper<IStatisticsHandler> statisticsHelper,
        HandlerHelper<ILogHandler> logHelper,
        ConfigWrapper? config = null,
        ISerializer<TKey>? keySerializer = null,
        ISerializer<TValue>? valueSerializer = null,
        IAsyncSerializer<TKey>? asyncKeySerializer = null,
        IAsyncSerializer<TValue>? asyncValueSerializer = null)
        : base(config?.Values)
    {
        ErrorHandler = errorHelper.Resolve(x => x.OnError, ErrorHandler);
        StatisticsHandler = statisticsHelper.Resolve(x => x.OnStatistics, StatisticsHandler);
        LogHandler = logHelper.Resolve(x => x.OnLog, LogHandler);

        KeySerializer = keySerializer;
        ValueSerializer = valueSerializer;

        // Setting both types of serializers is an error.
        if (keySerializer == null) AsyncKeySerializer = asyncKeySerializer;
        if (valueSerializer == null) AsyncValueSerializer = asyncValueSerializer;

        if (Config != null)
        {
            foreach (var kvp in Config)
            {
                ClientConfig[kvp.Key] = kvp.Value;
            }
        }

        Config = ClientConfig;
    }
}
