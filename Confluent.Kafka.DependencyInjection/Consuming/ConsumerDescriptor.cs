using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Consuming
{
    class ConsumerDescriptor<TKey, TValue> : IConsumerDescriptor<TKey, TValue>
    {
        private readonly IServiceCollection services;
        private readonly IConsumerProvider provider;

        public ConsumerDescriptor(IServiceCollection services, IConsumerProvider provider)
        {
            this.services = services;
            this.provider = provider;
        }

        IConsumerDescriptor<TKey, TValue> IClientDescriptor<IConsumer<TKey, TValue>, IConsumerDescriptor<TKey, TValue>>
            .AddConfiguration(IEnumerable<KeyValuePair<string, string>> configuration)
        {
            provider.Configuration = configuration;
            return this;
        }

        IConsumerDescriptor<TKey, TValue> IClientDescriptor<IConsumer<TKey, TValue>, IConsumerDescriptor<TKey, TValue>>.AddLogging()
        {
            provider.EnableLogging = true;
            return this;
        }

        IConsumerDescriptor<TKey, TValue> IClientDescriptor<IConsumer<TKey, TValue>, IConsumerDescriptor<TKey, TValue>>.AddErrorHandler<T>() =>
            AddService<IErrorHandler<IConsumer<TKey, TValue>>, T>();

        IConsumerDescriptor<TKey, TValue> IClientDescriptor<IConsumer<TKey, TValue>, IConsumerDescriptor<TKey, TValue>>.AddStatisticsHandler<T>() =>
            AddService<IStatisticsHandler<IConsumer<TKey, TValue>>, T>();

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.AddTopics(IEnumerable<string> topics)
        {
            provider.Topics = topics;
            return this;
        }

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.AddOffsetsCommittedHandler<T>() =>
            AddService<IOffsetsCommittedHandler<TKey, TValue>, T>();

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.SetKeyDeserializer<T>() =>
            AddService<IDeserializer<TKey>, T>();

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.SetValueDeserializer<T>() =>
            AddService<IDeserializer<TValue>, T>();

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.SetPartitionsAssignedHandler<T>() =>
            AddService<IPartitionsAssignedHandler<TKey, TValue>, T>();

        IConsumerDescriptor<TKey, TValue> IConsumerDescriptor<TKey, TValue>.SetPartitionsRevokedHandler<T>() =>
            AddService<IPartitionsRevokedHandler<TKey, TValue>, T>();

        private IConsumerDescriptor<TKey, TValue> AddService<TService, TImplementation>()
            where TService : class where TImplementation : class, TService
        {
            // Add as a factory in case implementation also implements other services.
            services.AddSingleton<TService>(sp => sp.GetRequiredService<TImplementation>());
            services.TryAddSingleton<TImplementation>();
            return this;
        }
    }
}
