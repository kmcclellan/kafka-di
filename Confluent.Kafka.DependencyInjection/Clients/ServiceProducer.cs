namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceProducer<TKey, TValue> : ScopedProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ProducerConfig config)
        : base(scopes, config)
    {
    }
}
