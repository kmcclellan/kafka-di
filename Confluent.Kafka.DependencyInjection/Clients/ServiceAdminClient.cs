namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceAdminClient : ScopedAdminClient
{
    public ServiceAdminClient(IServiceScopeFactory scopes, AdminClientConfig config)
        : base(scopes, config)
    {
    }
}
