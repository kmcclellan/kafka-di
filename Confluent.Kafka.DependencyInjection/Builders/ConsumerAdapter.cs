namespace Confluent.Kafka.DependencyInjection.Builders;

using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.SyncOverAsync;

using System.Collections.Generic;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ConsumerAdapter<TKey, TValue> : ConsumerBuilder<TKey, TValue>
{
    public IDictionary<string, string> ClientConfig { get; } = new Dictionary<string, string>();

    public ConsumerAdapter(
        HandlerHelper<IErrorHandler> errorHelper,
        HandlerHelper<IStatisticsHandler> statisticsHelper,
        HandlerHelper<ILogHandler> logHelper,
        HandlerHelper<IPartitionsAssignedHandler> assignHelper,
        HandlerHelper<IPartitionsRevokedHandler> revokeHelper,
        HandlerHelper<IOffsetsCommittedHandler> commitHelper,
        ConfigWrapper? config = null,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null,
        IAsyncDeserializer<TKey>? asyncKeyDeserializer = null,
        IAsyncDeserializer<TValue>? asyncValueDeserializer = null)
        : base(config?.Values)
    {
        ErrorHandler = errorHelper.Resolve(x => x.OnError, ErrorHandler);
        StatisticsHandler = statisticsHelper.Resolve(x => x.OnStatistics, StatisticsHandler);
        LogHandler = logHelper.Resolve(x => x.OnLog, LogHandler);
        PartitionsAssignedHandler = assignHelper.Resolve(x => x.OnPartitionsAssigned, PartitionsAssignedHandler);
        PartitionsRevokedHandler = revokeHelper.Resolve(x => x.OnPartitionsRevoked, PartitionsRevokedHandler);
        OffsetsCommittedHandler = commitHelper.Resolve(x => x.OnOffsetsCommitted, OffsetsCommittedHandler);
        KeyDeserializer = keyDeserializer ?? asyncKeyDeserializer?.AsSyncOverAsync();
        ValueDeserializer = valueDeserializer ?? asyncValueDeserializer?.AsSyncOverAsync();

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
