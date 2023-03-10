namespace Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections;
using System.Collections.Concurrent;
using System.Text;

sealed class KafkaBuilderFactory
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        PartitionsLost = new(12, nameof(PartitionsLost)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    readonly KafkaClientOptions options;
    readonly ILoggerFactory? loggers;

    public KafkaBuilderFactory(IOptionsSnapshot<KafkaClientOptions> options, ILoggerFactory? loggers = null)
    {
        this.options = options.Value;
        this.loggers = loggers;
    }

    public ProducerBuilder<TKey, TValue> CreateProduce<TKey, TValue>()
    {
        return this.Create(
            x => new ProducerBuilder<TKey, TValue>(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y),
            (x, y) => x.SetLogHandler(y),
            this.loggers?.CreateLogger("Confluent.Kafka.Producer"));
    }

    public ConsumerBuilder<TKey, TValue> CreateConsume<TKey, TValue>()
    {
        var clientLogger = this.loggers?.CreateLogger("Confluent.Kafka.Consumer");
        var builder = this.Create(
            x => new ConsumerBuilder<TKey, TValue>(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y),
            (x, y) => x.SetLogHandler(y),
            clientLogger);

        if (this.options.RebalanceHandler != null || clientLogger != null)
        {
            builder.SetPartitionsAssignedHandler(
                (client, partitions) =>
                {
                    var offsets = partitions.Select(x => new TopicPartitionOffset(x, Offset.Unset));
                    offsets = this.options.RebalanceHandler?.Invoke(client, new(offsets.ToArray())) ?? offsets;

                    if (clientLogger != null)
                    {
                        LogKafka(clientLogger, PartitionsAssigned, null, "Kafka partitions assigned", client, offsets);
                    }

                    return offsets;
                });

            builder.SetPartitionsRevokedHandler(
                (client, offsets) =>
                {
                    var result = this.options.RebalanceHandler?.Invoke(client, new(offsets, revoked: true)) ?? offsets;

                    if (clientLogger != null)
                    {
                        LogKafka(clientLogger, PartitionsRevoked, null, "Kafka partitions revoked", client, result);
                    }

                    return result;
                });

            builder.SetPartitionsLostHandler(
                (client, offsets) =>
                {
                    var result = this.options.RebalanceHandler?.Invoke(client, new(offsets, lost: true)) ?? offsets;

                    if (clientLogger != null)
                    {
                        LogKafka(clientLogger, PartitionsLost, null, "Kafka partitions lost", client, result);
                    }

                    return result;
                });
        }

        if (this.options.CommitHandler != null || clientLogger != null)
        {
            builder.SetOffsetsCommittedHandler(
                (client, offsets) =>
                {
                    this.options.CommitHandler?.Invoke(client, offsets);

                    if (clientLogger != null)
                    {
                        if (offsets.Error.IsError)
                        {
                            LogKafka(clientLogger, default, offsets.Error, "Kafka commit error", client);
                        }
                        else
                        {
                            foreach (var group in offsets.Offsets.GroupBy(x => x.Error, x => x.TopicPartitionOffset))
                            {
                                if (group.Key.IsError)
                                {
                                    LogKafka(clientLogger, default, group.Key, "Kafka commit error", client);
                                }
                                else
                                {
                                    LogKafka(
                                        clientLogger,
                                        OffsetsCommitted,
                                        null,
                                        "Kafka offset commit",
                                        client,
                                        group);
                                }
                            }
                        }
                    }
                });
        }

        return builder;
    }

    public AdminClientBuilder CreateAdmin()
    {
        return this.Create(
            x => new AdminClientBuilder(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y),
            (x, y) => x.SetLogHandler(y),
            this.loggers?.CreateLogger("Confluent.Kafka.AdminClient"));
    }

    static void LogKafka(
        ILogger logger,
        EventId eventId,
        Error? error,
        string message,
        IClient client,
        IEnumerable<TopicPartitionOffset>? offsets = null)
    {
        logger.Log(
            error != null ? error.IsFatal ? LogLevel.Critical : LogLevel.Error : LogLevel.Information,
            eventId,
            new KafkaLogValues(message, client.Name, offsets?.ToArray()),
            error != null ? new KafkaException(error) : null,
            (x, y) => x.ToString());
    }

    TBuilder Create<TBuilder>(
        Func<IEnumerable<KeyValuePair<string, string>>, TBuilder> initialize,
        Action<IKafkaClientSetup, TBuilder> apply,
        Action<TBuilder, Action<IClient, Error>> configureErrors,
        Action<TBuilder, Action<IClient, string>> configureStats,
        Action<TBuilder, Action<IClient, string>> configureAuth,
        Action<TBuilder, Action<IClient, LogMessage>> configureLogs,
        ILogger? clientLogger)
    {
        var builder = initialize(this.options.Properties);

        foreach (var setup in this.options.Setups)
        {
            apply(setup, builder);
        }

        if (this.loggers != null)
        {
            var libLoggers = new ConcurrentDictionary<string, ILogger>();

            configureLogs(
                builder,
                (client, message) =>
                {
                    if (!libLoggers.TryGetValue(message.Facility, out ILogger? logger))
                    {
                        logger = this.loggers.CreateLogger("rdkafka#" + message.Facility);
                        libLoggers[message.Facility] = logger;
                    }

                    logger.Log(
                        (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                        default,
                        new KafkaLogValues(message.Message, message.Name, null),
                        null,
                        (x, y) => x.ToString());
                });
        }

        if (this.options.ErrorHandler != null || clientLogger != null)
        {
            configureErrors(
                builder,
                (client, error) =>
                {
                    this.options.ErrorHandler?.Invoke(client, error);

                    if (clientLogger != null)
                    {
                        LogKafka(clientLogger, default, error, "Kafka client error", client);
                    }
                });
        }

        if (this.options.StatisticsHandler != null)
        {
            configureStats(builder, this.options.StatisticsHandler);
        }

        if (this.options.AuthenticateHandler != null)
        {
            configureAuth(builder, this.options.AuthenticateHandler);
        }

        return builder;
    }

    readonly struct KafkaLogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        readonly string message;
        readonly string client;
        readonly TopicPartitionOffset[]? offsets;

        public KafkaLogValues(string message, string client, TopicPartitionOffset[]? offsets)
        {
            this.message = message;
            this.client = client;
            this.offsets = offsets;
        }

        public int Count => this.offsets == null ? 1 : 2;

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                return (index) switch
                {
                    0 => new("KafkaClient", this.client),
                    1 => new("KafkaOffsets", this.offsets),
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < this.Count;)
            {
                yield return this[i++];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append('[')
                .Append(this.client)
                .Append(']')
                .Append(' ')
                .Append(this.message);

            if (this.offsets != null)
            {
                builder.AppendLine();

                foreach (var tpo in this.offsets)
                {
                    builder.Append(' ', 2);
                    builder.AppendLine(tpo.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
