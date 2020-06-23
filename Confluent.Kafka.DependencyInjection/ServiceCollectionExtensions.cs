using System.Collections.Generic;
using Confluent.Kafka.DependencyInjection.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Confluent.Kafka.DependencyInjection
{
    /// <summary>
    /// Extensions to configure Kafka producers and consumers as services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IKafkaFactory"/> to the services.
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
            IEnumerable<KeyValuePair<string, string>> configuration) =>
                services.AddSingleton<IKafkaFactory, KafkaFactory>()
                    .AddTransient(typeof(ProducerAdapter<,>))
                    .AddTransient(typeof(ConsumerAdapter<,>))
                    .AddSingleton(new ConfigWrapper(configuration));
    }
}
