namespace Confluent.Kafka.DependencyInjection.Handlers.Default;

using Confluent.Kafka.DependencyInjection.Logging;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

sealed class GlobalHandler : IErrorHandler, ILogHandler
{
    readonly ConcurrentDictionary<string, ILogger> libLoggers = new();

    readonly ILoggerFactory factory;
    readonly ILogger<GlobalHandler> logger;

    public GlobalHandler(ILoggerFactory factory)
    {
        this.factory = factory;
        this.logger = factory.CreateLogger<GlobalHandler>();
    }

    public void OnError(IClient client, Error error) =>
        this.logger.LogKafkaError(client, error);

    public void OnLog(IClient client, LogMessage message)
    {
        if (!this.libLoggers.TryGetValue(message.Facility, out var logger))
        {
            logger = this.factory.CreateLogger($"rdkafka|{message.Facility}");
            this.libLoggers[message.Facility] = logger;
        }

        logger.Log(
            (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
            0,
            new KafkaLogValues(message.Name, message.Message),
            null,
            (x, y) => x.ToString());
    }
}
