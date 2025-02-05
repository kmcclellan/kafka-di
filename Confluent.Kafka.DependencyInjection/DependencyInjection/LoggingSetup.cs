namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

sealed class LoggingSetup(Action<IClient, LogMessage> action) : IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        builder.SetLogHandler(action);
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        builder.SetLogHandler(action);
    }

    public void Apply(AdminClientBuilder builder)
    {
        builder.SetLogHandler(action);
    }
}
