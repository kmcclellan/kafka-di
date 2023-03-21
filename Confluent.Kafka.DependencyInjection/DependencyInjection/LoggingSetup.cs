namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

sealed class LoggingSetup : IClientBuilderSetup
{
    readonly Action<IClient, LogMessage> action;

    public LoggingSetup(Action<IClient, LogMessage> action)
    {
        this.action = action;
    }

    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        builder.SetLogHandler(this.action);
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        builder.SetLogHandler(this.action);
    }

    public void Apply(AdminClientBuilder builder)
    {
        builder.SetLogHandler(this.action);
    }
}
