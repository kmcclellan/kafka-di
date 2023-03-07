namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

sealed class KafkaFactory : IKafkaFactory
{
    readonly IServiceScopeFactory scopes;

    public KafkaFactory(IServiceScopeFactory scopes)
    {
        this.scopes = scopes;
    }

    public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ScopedProducer<TKey, TValue>(scopes, configuration);
    }

    public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ScopedConsumer<TKey, TValue>(scopes, configuration);
    }
}
