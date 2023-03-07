namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

sealed class ServiceAdminClient : ScopedAdminClient
{
    public ServiceAdminClient(IServiceScopeFactory scopes, AdminClientConfig config)
        : base(scopes, config)
    {
    }
}
