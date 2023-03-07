namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

sealed class ServiceProducer<TKey, TValue> : ScopedProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ProducerConfig config)
        : base(scopes, config)
    {
    }
}
