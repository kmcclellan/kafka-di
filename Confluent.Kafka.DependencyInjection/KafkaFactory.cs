namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Builders;
using Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class KafkaFactory : IKafkaFactory
{
    readonly IServiceScopeFactory scopes;
    readonly ConfigWrapper? config;

    public KafkaFactory(IServiceScopeFactory scopes, ConfigWrapper? config = null)
    {
        this.scopes = scopes;
        this.config = config;
    }

    public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ScopedProducer<TKey, TValue>(scopes, Merge(configuration));
    }

    public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ScopedConsumer<TKey, TValue>(scopes, Merge(configuration));
    }

    IEnumerable<KeyValuePair<string, string>>? Merge(IEnumerable<KeyValuePair<string, string>>? configuration)
    {
        return configuration != null
            ? this.config?.Values.Concat(configuration) ?? configuration
            : null;
    }
}
