using Confluent.Kafka.DependencyInjection;
using Confluent.Kafka.DependencyInjection.Consuming;
using Confluent.Kafka.Hosting.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;

namespace Confluent.Kafka.Hosting
{
    /// <summary>
    /// Extensions to facilitate Kafka service hosting.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Configures the host to consume from Kafka.
        /// </summary>
        /// <param name="hostBuilder">The extended builder.</param>
        /// <param name="configure">A delegate to configure the consumer.</param>
        /// <returns>The same instance for chaining.</returns>
        public static IHostBuilder UseConsumer<TConsumer, TKey, TValue>(this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IConsumerDescriptor<TKey, TValue>> configure)
            where TConsumer : class, IHostedConsumer<TKey, TValue> =>
                hostBuilder.ConfigureServices((context, services) =>
                {
                    services.AddHostedConsumer<TConsumer, TKey, TValue>();
                    configure(context, services.AddConsumer<TKey, TValue>());
                });

        /// <summary>
        /// Adds an <see cref="IHostedConsumer{TKey, TValue}"/> to the services.
        /// </summary>
        /// <typeparam name="TConsumer">The consumer type.</typeparam>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        /// <param name="services">The extended services.</param>
        /// <returns>The same instance for chaining.</returns>
        public static IServiceCollection AddHostedConsumer<TConsumer, TKey, TValue>(this IServiceCollection services)
            where TConsumer : class, IHostedConsumer<TKey, TValue>
        {
            services.TryAddTransient<TConsumer>();
            services.AddTransient<IHostedConsumer<TKey, TValue>>(sp => sp.GetRequiredService<TConsumer>());
            services.AddHostedWorker<ConsumerHost<TKey, TValue>>();
            return services;
        }

        /// <summary>
        /// Adds an <see cref="IHostedWorker"/> to the services.
        /// </summary>
        /// <remarks>
        /// The work will be performed repeatedly until application stops or exception.
        /// </remarks>
        /// <param name="services">The extended services.</param>
        /// <returns>The same instance for chaining.</returns>
        public static IServiceCollection AddHostedWorker<T>(this IServiceCollection services)
            where T : class, IHostedWorker
        {
            services.TryAddTransient<T>();
            services.AddHostedService<WorkerService<T>>();
            return services;
        }
    }
}
