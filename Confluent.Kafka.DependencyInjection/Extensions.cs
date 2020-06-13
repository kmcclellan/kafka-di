using Confluent.Kafka.DependencyInjection.Consuming;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Confluent.Kafka.DependencyInjection
{
    /// <summary>
    /// Extensions to facilitate injection of Kafka consumers and producers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds a <see cref="IConsumer{TKey, TValue}"/> to the services.
        /// </summary>
        /// <typeparam name="TKey">The consumer key type.</typeparam>
        /// <typeparam name="TValue">The consumer value type.</typeparam>
        /// <param name="services">The extended services.</param>
        /// <returns>A descriptor used to configure the consumer.</returns>
        public static IConsumerDescriptor<TKey, TValue> AddConsumer<TKey, TValue>(this IServiceCollection services)
        {
            var provider = new ConsumerProvider<TKey, TValue>();
            services.AddSingleton(provider.Build);
            return new ConsumerDescriptor<TKey, TValue>(services, provider);
        }

        internal static IServiceProvider TryService<T>(this IServiceProvider provider, Action<T> action)
        {
            var service = provider.GetService<T>();
            if (service != null) action(service);
            return provider;
        }
    }
}
