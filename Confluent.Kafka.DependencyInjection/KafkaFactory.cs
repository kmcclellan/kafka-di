namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Builders;
using Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

using System;
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
        return new ProducerAdapter<TKey, TValue>(Merge(configuration), scopes.CreateScope(), dispose: true).Build();
    }

    public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        return new ConsumerAdapter<TKey, TValue>(Merge(configuration), scopes.CreateScope(), dispose: true).Build();
    }

    IEnumerable<KeyValuePair<string, string>> Merge(IEnumerable<KeyValuePair<string, string>>? overrides)
    {
        return overrides != null
            ? config?.Values.Concat(overrides) ?? overrides
            : config?.Values ?? throw new InvalidOperationException("Configuration is required.");
    }
}
