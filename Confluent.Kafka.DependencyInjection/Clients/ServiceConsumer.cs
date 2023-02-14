namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceConsumer<TReceiver, TKey, TValue> : ServiceConsumer<TKey, TValue>
{
    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(scopes, global?.Values.Concat(config.Values) ?? config.Values)
    {
    }
}

class ServiceConsumer<TKey, TValue> : ScopedConsumer<TKey, TValue>
{
    bool closed;

    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values)
    {
    }

    internal ServiceConsumer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>> config)
        : this(scopes.CreateScope(), config)
    {
    }

    ServiceConsumer(IServiceScope scope, IEnumerable<KeyValuePair<string, string>> config)
        : base(new ConsumerAdapter<TKey, TValue>(scope.ServiceProvider, config).Build(), scope)
    {
    }

    public override void Close()
    {
        base.Close();
        this.closed = true;
    }

    public override void Dispose()
    {
        if (!closed)
        {
            // Close when disposed by ServiceProvider.
            this.Close();
        }

        base.Dispose();
    }
}
