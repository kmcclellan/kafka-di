using System;
using System.Collections.Generic;
using Confluent.Kafka.DependencyInjection.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Confluent.Kafka.DependencyInjection
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class KafkaFactory : IKafkaFactory, IDisposable
    {
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly IServiceScopeFactory scopes;

        public KafkaFactory(IServiceScopeFactory scopes)
        {
            this.scopes = scopes;
        }

        public IProducer<TKey, TValue> CreateProducer<TKey, TValue>() =>
            CreateClient<IProducer<TKey, TValue>, ProducerAdapter<TKey, TValue>>();

        public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>() =>
            CreateClient<IConsumer<TKey, TValue>, ConsumerAdapter<TKey, TValue>>();

        TClient CreateClient<TClient, TBuilder>() where TBuilder : IBuilderAdapter<TClient>
        {
            // Create shared scope for handlers/converters.
            var scope = scopes.CreateScope();
            disposables.Add(scope);

            return scope.ServiceProvider
                .GetRequiredService<TBuilder>()
                .Build();
        }

        public void Dispose()
        {
            foreach (var x in disposables) x.Dispose();
        }
    }
}
