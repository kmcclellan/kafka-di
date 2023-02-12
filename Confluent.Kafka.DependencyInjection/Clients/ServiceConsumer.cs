namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceConsumer<TReceiver, TKey, TValue> : ServiceConsumer<TKey, TValue>
{
    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(global?.Values.Concat(config.Values) ?? config.Values, scopes.CreateScope())
    {
    }
}

class ServiceConsumer<TKey, TValue> : ScopedConsumer<TKey, TValue>
{
    bool closed;

    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(config.Values, scopes.CreateScope())
    {
    }

    protected ServiceConsumer(IEnumerable<KeyValuePair<string, string>> config, IServiceScope scope)
        : base(new ConsumerAdapter<TKey, TValue>(config, scope, dispose: false).Build(), scope)
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
