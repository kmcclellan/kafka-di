using System;
using System.Collections.Generic;
using Confluent.Kafka.DependencyInjection.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Confluent.Kafka.DependencyInjection
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    sealed class KafkaFactory : IKafkaFactory, IDisposable
    {
        readonly List<IDisposable> disposables = new();
        readonly IServiceScopeFactory scopes;

        public KafkaFactory(IServiceScopeFactory scopes)
        {
            this.scopes = scopes;
        }

        public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(
            IEnumerable<KeyValuePair<string, string>>? configuration = null) =>
                CreateClient<IProducer<TKey, TValue>, ProducerAdapter<TKey, TValue>>(configuration);

        public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
            IEnumerable<KeyValuePair<string, string>>? configuration = null) =>
                CreateClient<IConsumer<TKey, TValue>, ConsumerAdapter<TKey, TValue>>(configuration);

        TClient CreateClient<TClient, TBuilder>(IEnumerable<KeyValuePair<string, string>>? config)
            where TBuilder : IBuilderAdapter<TClient>
        {
            // Create shared scope for handlers/converters.
            var scope = scopes.CreateScope();
            disposables.Add(scope);

            var builder = scope.ServiceProvider.GetRequiredService<TBuilder>();

            if (config != null)
            {
                foreach (var kvp in config)
                {
                    builder.ClientConfig[kvp.Key] = kvp.Value;
                }
            }

            return builder.Build();
        }

        public void Dispose()
        {
            foreach (var x in disposables) x.Dispose();
        }
    }
}
