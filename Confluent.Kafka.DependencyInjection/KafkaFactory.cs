namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
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
        return new ServiceProducer<TKey, TValue>(scopes, configuration);
    }

    public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ServiceConsumer<TKey, TValue>(scopes, configuration, closeOnDispose: false);
    }
}
