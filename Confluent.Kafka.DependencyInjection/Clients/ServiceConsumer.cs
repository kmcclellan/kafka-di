namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceConsumer<TKey, TValue> : ScopedConsumer<TKey, TValue>
{
    bool closed;

    public ServiceConsumer(IServiceScopeFactory scopes, ConsumerConfig config)
        : base(scopes, config)
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
