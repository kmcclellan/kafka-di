namespace Confluent.Kafka.DependencyInjection.Logging;

using Microsoft.Extensions.Logging;

static class LoggerExtensions
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    public static void LogKafkaAssignment(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        LogKafka(
            logger,
            PartitionsAssigned,
            default,
            "Kafka partitions assigned",
            client,
            offsets);
    }

    public static void LogKafkaRevocation(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        LogKafka(
            logger,
            PartitionsRevoked,
            default,
            "Kafka partitions revoked",
            client,
            offsets);
    }

    public static void LogKafkaCommit(
        this ILogger logger,
        IClient client,
        IEnumerable<TopicPartitionOffset> offsets)
    {
        LogKafka(
            logger,
            OffsetsCommitted,
            default,
            "Kafka offsets committed",
            client,
            offsets);
    }

    public static void LogKafkaError(this ILogger logger, IClient client, Error error)
    {
        LogKafka(
            logger,
            default,
            error,
            "Kafka client error",
            client);
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
}
