namespace Confluent.Kafka.DependencyInjection
{
    using Microsoft.Extensions.Logging;

    sealed class LoggingBuilderSetup : IClientBuilderSetup
    {
        readonly ILoggerFactory factory;

        public LoggingBuilderSetup(ILoggerFactory factory = null)
        {
            this.factory = factory;
        }

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
}
