namespace Confluent.Kafka.DependencyInjection.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// Extensions for logging common Kafka scenarios.
/// </summary>
public static class LoggerExtensions
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    /// <summary>
    /// Logs a Kafka partition assignment.
    /// </summary>
    /// <param name="logger">The extended logger.</param>
    /// <param name="client">The associated Kafka client.</param>
    /// <param name="offsets">The assigned Kafka partitions/offsets.</param>
    public static void LogKafkaAssignment(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (offsets is null) throw new ArgumentNullException(nameof(offsets));

        logger.Log(
            LogLevel.Information,
            PartitionsAssigned,
            new KafkaLogValues(client.Name, "Partitions assigned", offsets.ToList()),
            null,
            (x, _) => x.ToString());
    }

    /// <summary>
    /// Logs a Kafka partition revocation.
    /// </summary>
    /// <param name="logger">The extended logger.</param>
    /// <param name="client">The associated Kafka client.</param>
    /// <param name="offsets">The revoked Kafka partitions/offsets.</param>
    public static void LogKafkaRevocation(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (offsets is null) throw new ArgumentNullException(nameof(offsets));

        logger.Log(
            LogLevel.Information,
            PartitionsRevoked,
            new KafkaLogValues(client.Name, "Partitions revoked", offsets.ToList()),
            null,
            (x, _) => x.ToString());
    }

    /// <summary>
    /// Logs a Kafka commit.
    /// </summary>
    /// <param name="logger">The extended logger.</param>
    /// <param name="client">The associated Kafka client.</param>
    /// <param name="offsets">The committed Kafka offsets.</param>
    public static void LogKafkaCommit(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (offsets is null) throw new ArgumentNullException(nameof(offsets));

        logger.Log(
            LogLevel.Information,
            OffsetsCommitted,
            new KafkaLogValues(client.Name, "Offsets committed", offsets.ToList()),
            null,
            (x, _) => x.ToString());
    }

    /// <summary>
    /// Logs a Kafka error.
    /// </summary>
    /// <param name="logger">The extended logger.</param>
    /// <param name="client">The associated Kafka client.</param>
    /// <param name="error">The Kafka error.</param>
    public static void LogKafkaError(this ILogger logger, IClient client, Error error)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (error is null) throw new ArgumentNullException(nameof(error));

        logger.Log(
            error.IsFatal ? LogLevel.Critical : LogLevel.Error,
            default,
            new KafkaLogValues(client.Name, error.ToString()),
            new KafkaException(error),
            (x, y) => x.ToString());
    }
}
