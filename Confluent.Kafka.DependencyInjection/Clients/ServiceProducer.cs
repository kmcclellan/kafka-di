namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceProducer<TReceiver, TKey, TValue> : ServiceProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(scopes, global?.Values.Concat(config.Values) ?? config.Values)
    {
    }
}

class ServiceProducer<TKey, TValue> : ScopedProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values)
    {
    }

    protected ServiceProducer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>> config)
        : base(scopes, config)
    {
    }
}
