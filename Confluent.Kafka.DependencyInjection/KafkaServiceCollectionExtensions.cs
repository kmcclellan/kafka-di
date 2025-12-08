namespace Confluent.Kafka
{
    using Confluent.Kafka.DependencyInjection;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    using System;

    /// <summary>
    /// Extensions to configure Kafka clients as services.
    /// </summary>
    public static class KafkaServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IProducer{TKey, TValue}"/>, <see cref="IConsumer{TKey, TValue}"/>, and <see cref="IAdminClient"/> to the services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same services, for chaining.</returns>
        public static IServiceCollection AddKafkaClient(this IServiceCollection services)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(services, nameof(services));
#else
            if (services == null) throw new ArgumentNullException(nameof(services));
#endif

            services.TryAddSingleton(typeof(IProducer<,>), typeof(GlobalProducer<,>));
            services.TryAddSingleton(typeof(IConsumer<,>), typeof(GlobalConsumer<,>));
            services.TryAddSingleton<IAdminClient, GlobalAdminClient>();

            services.AddOptions();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IClientConfigProvider, DefaultConfigProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IClientBuilderSetup, LoggingBuilderSetup>());

            AddClientConfig<AdminClientConfig, ConfigureClientProperties>(services);
            AddClientConfig<ConsumerConfig, ConfigureClientProperties>(services);
            AddClientConfig<ProducerConfig, ConfigureClientProperties>(services);

            return services;
        }

        static void AddClientConfig<TConfig, TConfigure>(IServiceCollection services)
            where TConfig : class
            where TConfigure : class, IConfigureOptions<TConfig>
        {
            services.TryAddTransient(x => x.GetRequiredService<IOptionsFactory<TConfig>>().Create(Options.DefaultName));
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<TConfig>, TConfigure>());
        }
    }
}
