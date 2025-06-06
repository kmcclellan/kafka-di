namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

sealed class ConfigureClientLogging(ILoggerFactory? factory = null) :
    ConfigureNamedOptions<KafkaClientOptions>(null, factory != null ? x => Configure(factory, x) : null)
{
    static readonly EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
        PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
        PartitionsLost = new(12, nameof(PartitionsLost)),
        OffsetsCommitted = new(20, nameof(OffsetsCommitted));

    public static void Configure(ILoggerFactory factory, KafkaClientOptions options)
    {
        options.Setup(new LoggingSetup(factory));
    }

    sealed class LoggingSetup(ILoggerFactory factory) : IClientBuilderSetup
    {
        public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
        {
            builder.AddLogging(factory);
        }

        public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
        {
            builder.AddLogging(factory);
        }

        public void Apply(AdminClientBuilder builder)
        {
            builder.AddLogging(factory);
        }
    }
}
