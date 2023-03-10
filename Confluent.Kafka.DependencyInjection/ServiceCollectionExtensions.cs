namespace Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extensions to configure Kafka producers and consumers as services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IProducer{TKey, TValue}"/>, <see cref="IConsumer{TKey, TValue}"/>, and <see cref="IAdminClient"/> to the services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate to configure Kafka clients.</param>
    /// <returns>The same instance for chaining.</returns>
    public static IServiceCollection AddKafkaClient(
        this IServiceCollection services,
        Action<KafkaClientOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddOptions<KafkaClientOptions>()
            .Configure<IServiceProvider>((x, y) => y.GetService<IConfiguration>()?.GetSection("Kafka").Bind(x));

        if (configure != null)
        {
            services.Configure(configure);
        }

        // Use shared scope for builder and its dependencies (e.g. serializers, handlers).
        services.TryAddScoped<KafkaBuilderFactory>();

        services.TryAddSingleton(typeof(IProducer<,>), typeof(DIProducer<,>));
        services.TryAddSingleton(typeof(IConsumer<,>), typeof(DIConsumer<,>));
        services.TryAddSingleton<IAdminClient, DIAdminClient>();

        return services;
    }
}
