namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Logging;

sealed class LoggingBuilderSetup(ILoggerFactory? factory = null) : IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (factory != null)
        {
            builder.AddLogging(factory);
        }
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        if (factory != null)
        {
            builder.AddLogging(factory);
        }
    }

    public void Apply(AdminClientBuilder builder)
    {
        if (factory != null)
        {
            builder.AddLogging(factory);
        }
    }
}
