using System.Collections.Generic;
using Confluent.Kafka.DependencyInjection.Builders;
using Confluent.Kafka.DependencyInjection.Clients;
using Confluent.Kafka.DependencyInjection.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Confluent.Kafka.DependencyInjection
{
    /// <summary>
    /// Extensions to configure Kafka producers and consumers as services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IProducer{TKey, TValue}"/>, <see cref="IConsumer{TKey, TValue}"/>, and <see cref="IKafkaFactory"/> to the services.
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
            services.TryAddTransient(typeof(ProducerAdapter<,>));
            services.TryAddTransient(typeof(ConsumerAdapter<,>));
            services.TryAddTransient(typeof(HandlerHelper<>));

            if (configuration != null)
            {
                services.AddSingleton(typeof(IProducer<,>), typeof(ServiceProducer<,>))
                    .AddSingleton(typeof(IConsumer<,>), typeof(ServiceConsumer<,>));

                services.AddSingleton(new ConfigWrapper(configuration));
            }

            return services;
        }
    }
}
