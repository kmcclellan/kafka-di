namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;
using Confluent.Kafka.DependencyInjection.Handlers.Default;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;

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
        services.TryAddSingleton<IKafkaFactory, KafkaFactory>();
        services.AddAdapters();

        if (configuration != null)
        {
            services.AddSingleton(typeof(IProducer<,>), typeof(ServiceProducer<,>))
                .AddSingleton(typeof(IConsumer<,>), typeof(ServiceConsumer<,>));

            services.AddSingleton(new ConfigWrapper(configuration));
        }

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
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.TryAddSingleton(typeof(ServiceProducer<,,>));
        services.TryAddSingleton(typeof(ServiceConsumer<,,>));

        services.AddAdapters();
        services.AddSingleton(new ConfigWrapper<TImplementation>(configuration));

        // Factory allows us to use custom service provider for constructor args.
        var factory = ActivatorUtilities.CreateFactory(typeof(TImplementation), Array.Empty<Type>());
        var mappings = new Dictionary<Type, Type>
        {
            { typeof(IProducer<,>), typeof(ServiceProducer<,,>) },
            { typeof(IConsumer<,>), typeof(ServiceConsumer<,,>) },
        };

        object CreateClient(IServiceProvider sp)
        {
            // Mapper will resolve services using the correct receiver type.
            var mapper = new GenericServiceMapper<TImplementation>(sp, mappings);
            return factory(mapper, Array.Empty<object>());
        }

        services.Add(new ServiceDescriptor(typeof(TService), CreateClient, lifetime));
        return services;
    }

    static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        services.TryAddSingleton<IErrorHandler, GlobalHandler>();
        services.TryAddSingleton<ILogHandler, GlobalHandler>();
        services.TryAddSingleton<IPartitionsAssignedHandler, AssignmentHandler>();
        services.TryAddSingleton<IPartitionsRevokedHandler, AssignmentHandler>();
        services.TryAddSingleton<IOffsetsCommittedHandler, CommitHandler>();

        // These must be transient to consume scoped handlers.
        services.TryAddTransient(typeof(ProducerBuilder<,>), typeof(DIProducerBuilder<,>));
        services.TryAddTransient(typeof(ConsumerBuilder<,>), typeof(DIConsumerBuilder<,>));

        return services;
    }
}
