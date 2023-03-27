namespace Confluent.Kafka;

using Confluent.Kafka.DependencyInjection;
using Confluent.Kafka.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using MSOptions = Microsoft.Extensions.Options.Options;

/// <summary>
/// Extensions to configure Kafka clients as services.
/// </summary>
public static class KafkaServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IProducer{TKey, TValue}"/>, <see cref="IConsumer{TKey, TValue}"/>, and <see cref="IAdminClient"/> to the services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A builder for client options.</returns>
    public static OptionsBuilder<KafkaClientOptions> AddKafkaClient(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.TryAddSingleton<ClientScopeFactory>(
            provider =>
            {
                return (out KafkaClientOptions options) =>
                {
                    // Use shared scope for options dependencies (e.g. serializers, handlers).
                    var scope = provider.CreateScope();

                    options = scope.ServiceProvider
                        .GetRequiredService<IOptionsFactory<KafkaClientOptions>>()
                        .Create(MSOptions.DefaultName);

                    return scope;
                };
            });

        services.TryAddSingleton(typeof(IProducer<,>), typeof(ScopedProducer<,>));
        services.TryAddSingleton(typeof(IConsumer<,>), typeof(ScopedConsumer<,>));
        services.TryAddSingleton<IAdminClient, ScopedAdminClient>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<KafkaClientOptions>, ConfigureClientLogging>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<KafkaClientOptions>, ConfigureClientProperties>());

        return services.AddOptions<KafkaClientOptions>();
    }
}
