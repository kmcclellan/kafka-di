namespace Confluent.Kafka;

using Microsoft.Extensions.Logging;

using System.Collections;
using System.Collections.Concurrent;
using System.Text;

/// <summary>
/// Extensions of Kafka components to integrate with <c>Microsoft.Extensions.Logging</c>.
/// </summary>
public static class MSLoggingExtensions
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        PartitionsLost = new(12, nameof(PartitionsLost)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    /// <summary>
    /// Configures internal logging for admin clients.
    /// </summary>
    /// <param name="builder">The client builder.</param>
    /// <param name="factory">The logger factory.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static AdminClientBuilder AddLogging(this AdminClientBuilder builder, ILoggerFactory factory)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
#else
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
#endif

        var libLoggers = new ConcurrentDictionary<string, ILogger>();
        var clientLogger = default(ILogger?);

        builder.SetLogHandler((x, y) => HandleLog(factory, libLoggers, y));
        builder.SetErrorHandler((x, y) => HandleError(factory, ref clientLogger, x, y));

        return builder;
    }

    /// <summary>
    /// Configures internal logging for consumers.
    /// </summary>
    /// <param name="builder">The consumer builder.</param>
    /// <param name="factory">The logger factory.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static ConsumerBuilder<TKey, TValue> AddLogging<TKey, TValue>(
        this ConsumerBuilder<TKey, TValue> builder,
        ILoggerFactory factory)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
#else
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
#endif

        var libLoggers = new ConcurrentDictionary<string, ILogger>();
        var clientLogger = default(ILogger?);

        builder.SetLogHandler((x, y) => HandleLog(factory, libLoggers, y));
        builder.SetErrorHandler((x, y) => HandleError(factory, ref clientLogger, x, y));

        builder.SetPartitionsAssignedHandler(
            (client, partitions) =>
            {
                clientLogger ??= factory.CreateLogger(client.GetType());
                var offsets = partitions.Select(x => new TopicPartitionOffset(x, Offset.Unset));

                LogEvent(
                    clientLogger,
                    LogLevel.Information,
                    PartitionsAssigned,
                    new("Kafka partitions assigned", client.Name, offsets));
            });

        builder.SetPartitionsRevokedHandler(
            (client, offsets) =>
            {
                clientLogger ??= factory.CreateLogger(client.GetType());

                LogEvent(
                    clientLogger,
                    LogLevel.Information,
                    PartitionsRevoked,
                    new("Kafka partitions revoked", client.Name, offsets));
            });

        builder.SetPartitionsLostHandler(
            (client, offsets) =>
            {
                clientLogger ??= factory.CreateLogger(client.GetType());

                LogEvent(
                    clientLogger,
                    LogLevel.Information,
                    PartitionsLost,
                    new("Kafka partitions lost", client.Name, offsets));
            });

        builder.SetOffsetsCommittedHandler(
            (client, commit) =>
            {
                clientLogger ??= factory.CreateLogger(client.GetType());

                if (commit.Error.IsError)
                {
                    LogError(clientLogger, commit.Error, new("Kafka commit error", client.Name));
                }
                else
                {
                    foreach (var group in commit.Offsets.GroupBy(x => x.Error, x => x.TopicPartitionOffset))
                    {
                        var offsets = group.Where(x => x.Offset != Offset.Unset);

                        if (group.Key.IsError)
                        {
                            LogError(clientLogger, commit.Error, new("Kafka commit error", client.Name, offsets));
                        }
                        else
                        {
                            LogEvent(
                                clientLogger,
                                LogLevel.Information,
                                OffsetsCommitted,
                                new("Kafka offset commit", client.Name, offsets));
                        }
                    }
                }
            });

        return builder;
    }

    /// <summary>
    /// Configures internal logging for producers.
    /// </summary>
    /// <param name="builder">The producer builder.</param>
    /// <param name="factory">The logger factory.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static ProducerBuilder<TKey, TValue> AddLogging<TKey, TValue>(
        this ProducerBuilder<TKey, TValue> builder,
        ILoggerFactory factory)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
#else
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
#endif

        var libLoggers = new ConcurrentDictionary<string, ILogger>();
        var clientLogger = default(ILogger?);

        builder.SetLogHandler((x, y) => HandleLog(factory, libLoggers, y));
        builder.SetErrorHandler((x, y) => HandleError(factory, ref clientLogger, x, y));

        return builder;
    }

    static void HandleLog(
        ILoggerFactory factory,
        ConcurrentDictionary<string, ILogger> libLoggers,
        LogMessage message)
    {
        if (!libLoggers.TryGetValue(message.Facility, out ILogger? logger))
        {
            logger = factory.CreateLogger("rdkafka#" + message.Facility);
            libLoggers[message.Facility] = logger;
        }

        var level = (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging);
        LogEvent(logger, level, eventId: default, new(message.Message, message.Name));
    }

    static void HandleError(
        ILoggerFactory factory,
        ref ILogger? clientLogger,
        IClient client,
        Error error)
    {
        clientLogger ??= factory.CreateLogger(client.GetType());
        LogError(clientLogger, error, new("Kafka client error", client.Name));
    }

    static void LogEvent(ILogger logger, LogLevel level, EventId eventId, ClientLogValues values)
    {
        logger.Log(level, eventId, values, exception: null, (x, y) => x.ToString());
    }

    static void LogError(ILogger logger, Error error, ClientLogValues values)
    {
        logger.Log(
            error.IsFatal ? LogLevel.Critical : LogLevel.Error,
            eventId: default,
            values,
            exception: new KafkaException(error),
            (x, y) => x.ToString());
    }

    readonly struct ClientLogValues(
        string message,
        string client,
        IEnumerable<TopicPartitionOffset>? offsets = null) :
        IReadOnlyList<KeyValuePair<string, object?>>
    {
        public int Count => offsets == null ? 1 : 2;

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                return index switch
                {
                    0 => new("KafkaClient", client),
                    1 => new("KafkaOffsets", offsets),
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (var i = 0; i < this.Count;)
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
                .Append(client)
                .Append(']')
                .Append(' ')
                .Append(message);

            if (offsets != null)
            {
                builder.AppendLine();

                foreach (var tpo in offsets)
                {
                    builder.Append(' ', 2);
                    builder.AppendLine(tpo.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
