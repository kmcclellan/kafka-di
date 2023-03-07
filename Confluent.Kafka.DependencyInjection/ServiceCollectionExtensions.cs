namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.DependencyInjection.Handlers.Default;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extensions to configure Kafka producers and consumers as services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IKafkaFactory"/> and globally configured <see cref="IProducer{TKey, TValue}"/> and <see cref="IConsumer{TKey, TValue}"/> to the services.
    /// </summary>
    /// <remarks>
    /// See <see href="https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see> for supported configuration properties.
    /// </remarks>
    /// <seealso cref="ProducerConfig"/>
    /// <seealso cref="ConsumerConfig"/>
    /// <param name="services">The extended services.</param>
    /// <param name="configuration">Configuration properties used by producers/consumers.</param>
    /// <returns>The same instance for chaining.</returns>
    public static IServiceCollection AddKafkaClient(
        this IServiceCollection services,
        IEnumerable<KeyValuePair<string, string>>? configuration = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        if (configuration != null)
        {
            services.AddSingleton(new ConfigureKafka(configuration));
        }

        AddConfig<ProducerConfig>(services);
        AddConfig<ConsumerConfig>(services);
        AddConfig<AdminClientConfig>(services);

        services.TryAddSingleton<IErrorHandler, GlobalHandler>();
        services.TryAddSingleton<ILogHandler, GlobalHandler>();
        services.TryAddSingleton<IPartitionsAssignedHandler, AssignmentHandler>();
        services.TryAddSingleton<IPartitionsRevokedHandler, AssignmentHandler>();
        services.TryAddSingleton<IOffsetsCommittedHandler, CommitHandler>();

        services.TryAddTransient(typeof(ProducerBuilder<,>), typeof(DIProducerBuilder<,>));
        services.TryAddTransient(typeof(ConsumerBuilder<,>), typeof(DIConsumerBuilder<,>));
        services.TryAddTransient<AdminClientBuilder, DIAdminClientBuilder>();

        // These must be scoped to consume scoped config.
        services.TryAddScoped(typeof(IProducer<,>), typeof(ServiceProducer<,>));
        services.TryAddScoped(typeof(IConsumer<,>), typeof(ServiceConsumer<,>));
        services.TryAddScoped<IAdminClient, ServiceAdminClient>();

        services.TryAddSingleton<IKafkaFactory, KafkaFactory>();

        return services;
    }

    /// <summary>
    /// Adds a particular client implementation to the services.
    /// </summary>
    /// <remarks>
    /// <para>The container will provide the client with <see cref="IProducer{TKey, TValue}"/> and <see cref="IConsumer{TKey, TValue}"/> using this configuration.</para>
    /// <para>See <see href="https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see> for supported configuration properties.</para>
    /// <para>Configuration will be merged with <see cref="AddKafkaClient(IServiceCollection, IEnumerable{KeyValuePair{string, string}}?)"/>, if applicable.</para>
    /// </remarks>
    /// <seealso cref="ProducerConfig"/>
    /// <seealso cref="ConsumerConfig"/>
    /// <typeparam name="TClient">The client service and implementation type.</typeparam>
    /// <param name="services">The extended services.</param>
    /// <param name="configuration">Configuration properties used by producers/consumers.</param>
    /// <param name="lifetime">The lifetime of the client.</param>
    /// <returns>The same instance for chaining.</returns>
    public static IServiceCollection AddKafkaClient<TClient>(
        this IServiceCollection services,
        IEnumerable<KeyValuePair<string, string>> configuration,
        ServiceLifetime lifetime = ServiceLifetime.Scoped) =>
            services.AddKafkaClient<TClient, TClient>(configuration, lifetime);

    /// <summary>
    /// Adds a particular client implementation to the services.
    /// </summary>
    /// <remarks>
    /// <para>The container will provide the client with <see cref="IProducer{TKey, TValue}"/> and <see cref="IConsumer{TKey, TValue}"/> using this configuration.</para>
    /// <para>See <see href="https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md">librdkafka documentation</see> for supported configuration properties.</para>
    /// <para>Configuration will be merged with <see cref="AddKafkaClient(IServiceCollection, IEnumerable{KeyValuePair{string, string}}?)"/>, if applicable.</para>
    /// </remarks>
    /// <seealso cref="ProducerConfig"/>
    /// <seealso cref="ConsumerConfig"/>
    /// <typeparam name="TService">The client service type.</typeparam>
    /// <typeparam name="TImplementation">The client implementation type.</typeparam>
    /// <param name="services">The extended services.</param>
    /// <param name="configuration">Configuration properties used by producers/consumers.</param>
    /// <param name="lifetime">The lifetime of the client.</param>
    /// <returns>The same instance for chaining.</returns>
    public static IServiceCollection AddKafkaClient<TService, TImplementation>(
        this IServiceCollection services,
        IEnumerable<KeyValuePair<string, string>> configuration,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TImplementation : TService
    {
        AddKafkaClient(services);

        services.AddSingleton(x => new KafkaScope<TImplementation>(x.CreateScope(), configuration));

        services.Add(
            ServiceDescriptor.Describe(
                typeof(TService),
                x => x.GetRequiredService<KafkaScope<TImplementation>>().Resolve(),
                lifetime));

        return services;
    }

    static void AddConfig<T>(IServiceCollection services)
        where T : Config, new()
    {
        // Scopes allow us to resolve/override values used by builders.
        services.TryAddScoped(
            provider =>
            {
                var merged = new T();

                foreach (var setup in provider.GetServices<ConfigureKafka>())
                {
                    setup.Configure(merged);
                }

                return merged;
            });
    }

    class ConfigureKafka
    {
        readonly IEnumerable<KeyValuePair<string, string>> values;

        public ConfigureKafka(IEnumerable<KeyValuePair<string, string>> values)
        {
            this.values = values;
        }

        public void Configure(Config config)
        {
            foreach (var kvp in values)
            {
                config.Set(kvp.Key, kvp.Value);
            }
        }
    }

    class KafkaScope<TClient> : IDisposable
    {
        static readonly ObjectFactory Factory = ActivatorUtilities.CreateFactory(typeof(TClient), Array.Empty<Type>());

        readonly IServiceScope scope;

        public KafkaScope(IServiceScope scope, IEnumerable<KeyValuePair<string, string>> config)
        {
            this.scope = scope;

            Configure<ProducerConfig>(config);
            Configure<ConsumerConfig>(config);
            Configure<AdminClientConfig>(config);
        }

        void Configure<T>(IEnumerable<KeyValuePair<string, string>> config)
            where T : Config
        {
            var merged = scope.ServiceProvider.GetRequiredService<T>();

            foreach (var kvp in config)
            {
                merged.Set(kvp.Key, kvp.Value);
            }
        }

        public object Resolve()
        {
            return Factory(scope.ServiceProvider, Array.Empty<object>());
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
