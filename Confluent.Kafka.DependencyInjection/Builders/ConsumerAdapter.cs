using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.SyncOverAsync;

namespace Confluent.Kafka.DependencyInjection.Builders
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class ConsumerAdapter<TKey, TValue> : ConsumerBuilder<TKey, TValue>, IBuilderAdapter<IConsumer<TKey, TValue>>
    {
        public ConsumerAdapter(
            ConfigWrapper config,
            IErrorHandler? errorHandler = null,
            IStatisticsHandler? statisticsHandler = null,
            ILogHandler? logHandler = null,
            IPartitionsAssignedHandler? assignHandler = null,
            IPartitionsRevokedHandler? revokeHandler = null,
            IOffsetsCommittedHandler? commitHandler = null,
            IDeserializer<TKey>? keyDeserializer = null,
            IDeserializer<TValue>? valueDeserializer = null,
            IAsyncDeserializer<TKey>? asyncKeyDeserializer = null,
            IAsyncDeserializer<TValue>? asyncValueDeserializer = null)
                : base(config.Values)
        {
            if (errorHandler != null) ErrorHandler += errorHandler.OnError;
            if (statisticsHandler != null) StatisticsHandler += statisticsHandler.OnStatistics;
            if (logHandler != null) LogHandler += logHandler.OnLog;
            if (assignHandler != null) PartitionsAssignedHandler += assignHandler.OnPartitionsAssigned;
            if (revokeHandler != null) PartitionsRevokedHandler += revokeHandler.OnPartitionsRevoked;
            if (commitHandler != null) OffsetsCommittedHandler += commitHandler.OnOffsetsCommitted;
            KeyDeserializer = keyDeserializer ?? asyncKeyDeserializer?.AsSyncOverAsync();
            ValueDeserializer = valueDeserializer ?? asyncValueDeserializer?.AsSyncOverAsync();
        }
    }
}
