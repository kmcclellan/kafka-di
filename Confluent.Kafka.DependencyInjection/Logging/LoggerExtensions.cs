using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Confluent.Kafka.DependencyInjection.Logging
{
    static class LoggerExtensions
    {
        static Dictionary<ErrorCode, EventId>? eventIds;

        public static void LogKafkaMessage(this ILogger logger, LogMessage message) =>
            logger.Log(
                (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                "[{KafkaClient}] {Message}",
                message.Name,
                message.Message);

        public static void LogKafkaError(this ILogger logger, IClient client, Error error) =>
            logger.Log(
                error.IsFatal ? LogLevel.Critical : LogLevel.Error,
                GetErrorId(error.Code),
                "[{KafkaClient}] {Message}",
                client.Name,
                error.Reason);

        public static void LogKafkaOffsets(
            this ILogger logger,
            IClient client,
            EventId id,
            string message,
            IEnumerable<TopicPartitionOffset> offsets) =>
                logger.Log(
                    LogLevel.Information,
                    id,
                    new OffsetLogValues(client, offsets, message),
                    null,
                    (x, _) => x.ToString());

        public static void LogKafkaOffsets(
            this ILogger logger,
            IClient client,
            Error error,
            string message,
            IEnumerable<TopicPartitionOffset> offsets) =>
                logger.Log(
                    error.IsFatal ? LogLevel.Critical : LogLevel.Error,
                    GetErrorId(error.Code),
                    new OffsetLogValues(client, offsets, message),
                    null,
                    (x, _) => x.ToString());

        static EventId GetErrorId(ErrorCode code)
        {
            eventIds ??= Enum.GetValues(typeof(ErrorCode))
                .Cast<ErrorCode>()
                .ToDictionary(x => x, x => new EventId((int)x, x.ToString()));

            return eventIds[code];
        }
    }
}
