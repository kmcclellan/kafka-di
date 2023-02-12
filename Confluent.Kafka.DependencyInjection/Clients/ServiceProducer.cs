namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceProducer<TReceiver, TKey, TValue> : ServiceProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(global?.Values.Concat(config.Values) ?? config.Values, scopes.CreateScope())
    {
    }
}

class ServiceProducer<TKey, TValue> : ScopedProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(config.Values, scopes.CreateScope())
    {
    }

    protected ServiceProducer(IEnumerable<KeyValuePair<string, string>> config, IServiceScope scope)
        : base(new ProducerAdapter<TKey, TValue>(config, scope, dispose: false).Build(), scope)
    {
    }
}
