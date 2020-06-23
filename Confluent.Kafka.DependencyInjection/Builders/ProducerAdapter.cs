using Confluent.Kafka.DependencyInjection.Handlers;

namespace Confluent.Kafka.DependencyInjection.Builders
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class ProducerAdapter<TKey, TValue> : ProducerBuilder<TKey, TValue>, IBuilderAdapter<IProducer<TKey, TValue>>
    {
        public ProducerAdapter(
            ConfigWrapper config,
            IErrorHandler? errorHandler = null,
            IStatisticsHandler? statisticsHandler = null,
            ILogHandler? logHandler = null,
            ISerializer<TKey>? keySerializer = null,
            ISerializer<TValue>? valueSerializer = null,
            IAsyncSerializer<TKey>? asyncKeySerializer = null,
            IAsyncSerializer<TValue>? asyncValueSerializer = null)
                : base(config.Values)
        {
            if (errorHandler != null) ErrorHandler += errorHandler.OnError;
            if (statisticsHandler != null) StatisticsHandler += statisticsHandler.OnStatistics;
            if (logHandler != null) LogHandler += logHandler.OnLog;

            KeySerializer = keySerializer;
            ValueSerializer = valueSerializer;

            // Setting both types of serializers is an error.
            if (keySerializer == null) AsyncKeySerializer = asyncKeySerializer;
            if (valueSerializer == null) AsyncValueSerializer = asyncValueSerializer;
        }
    }
}
