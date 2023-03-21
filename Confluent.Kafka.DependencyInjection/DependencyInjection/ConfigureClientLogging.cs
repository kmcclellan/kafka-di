namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

class ConfigureClientLogging : ConfigureNamedOptions<KafkaClientOptions>
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        PartitionsLost = new(12, nameof(PartitionsLost)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    public ConfigureClientLogging(ILoggerFactory? factory = null)
        : base(null, factory != null ? x => Configure(factory, x) : null)
    {
    }

    public static void Configure(ILoggerFactory factory, KafkaClientOptions options)
    {
        var libLoggers = new ConcurrentDictionary<string, ILogger>();
        var clientLoggers = new ConcurrentDictionary<IClient, ILogger>();

        options.Setup(
            new LoggingSetup(
                (client, message) =>
                {
                    if (!libLoggers.TryGetValue(message.Facility, out ILogger? logger))
                    {
                        logger = factory.CreateLogger("rdkafka#" + message.Facility);
                        libLoggers[message.Facility] = logger;
                    }

                    logger.Log(
                        (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                        default,
                        new ClientLogValues(message.Message, message.Name, null),
                        null,
                        (x, y) => x.ToString());
                }));

        options.OnError((x, y) => LogKafka(x, "Kafka client error", error: y));

        options.OnRebalance(
            (client, rebalance) =>
            {
                if (rebalance.Revoked)
                {
                    LogKafka(client, "Kafka partitions revoked", PartitionsRevoked, offsets: rebalance.Offsets);
                }
                else if (rebalance.Lost)
                {
                    LogKafka(client, "Kafka partitions lost", PartitionsLost, offsets: rebalance.Offsets);
                }
                else
                {
                    LogKafka(client, "Kafka partitions assigned", PartitionsAssigned, offsets: rebalance.Offsets);
                }
            });

        options.OnCommit(
            (client, commit) =>
            {
                if (commit.Error.IsError)
                {
                    LogKafka(client, "Kafka commit error", error: commit.Error);
                }
                else
                {
                    foreach (var group in commit.Offsets.GroupBy(x => x.Error, x => x.TopicPartitionOffset))
                    {
                        if (group.Key.IsError)
                        {
                            LogKafka(client, "Kafka commit error", error: group.Key);
                        }
                        else
                        {
                            LogKafka(client, "Kafka offset commit", OffsetsCommitted, offsets: group);
                        }
                    }
                }
            });

        void LogKafka(
            IClient client,
            string message,
            EventId eventId = default,
            Error? error = null,
            IEnumerable<TopicPartitionOffset>? offsets = null)
        {
            if (!clientLoggers.TryGetValue(client, out var logger))
            {
                logger = factory.CreateLogger(client.GetType());
                clientLoggers[client] = logger;
            }

            logger.Log(
                error != null ? error.IsFatal ? LogLevel.Critical : LogLevel.Error : LogLevel.Information,
                eventId,
                new ClientLogValues(message, client.Name, offsets),
                error != null ? new KafkaException(error) : null,
                (x, y) => x.ToString());
        }
    }
}
