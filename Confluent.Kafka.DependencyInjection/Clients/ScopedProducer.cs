namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;
using Confluent.Kafka.DependencyInjection.Handlers;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class ScopedProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    readonly IProducer<TKey, TValue> producer;
    readonly IDisposable scope;

    public ScopedProducer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>>? config = null)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();
        this.producer = GetBuilder(scope.ServiceProvider, config).Build();
    }

    public Handle Handle => producer.Handle;

    public string Name => producer.Name;

    static ProducerBuilder<TKey, TValue> GetBuilder(
        IServiceProvider services,
        IEnumerable<KeyValuePair<string, string>>? config)
    {
       return config != null
            ? new ProducerAdapter<TKey, TValue>(
                new(config),
                services.GetServices<IErrorHandler>(),
                services.GetServices<IStatisticsHandler>(),
                services.GetServices<ILogHandler>(),
                services.GetService<ISerializer<TKey>>(),
                services.GetService<ISerializer<TValue>>(),
                services.GetService<IAsyncSerializer<TKey>>(),
                services.GetService<IAsyncSerializer<TValue>>())
            : services.GetRequiredService<ProducerBuilder<TKey, TValue>>();
    }

    public int AddBrokers(string brokers)
    {
        return producer.AddBrokers(brokers);
    }

    public void Produce(
        string topic,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        producer.Produce(topic, message, deliveryHandler);
    }

    public void Produce(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        producer.Produce(topicPartition, message, deliveryHandler);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
       return producer.ProduceAsync(topic, message, cancellationToken);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return producer.ProduceAsync(topicPartition, message, cancellationToken);
    }

    public void BeginTransaction()
    {
        producer.BeginTransaction();
    }

    public void CommitTransaction()
    {
        this.producer.CommitTransaction();
    }

    public void CommitTransaction(TimeSpan timeout)
    {
        producer.CommitTransaction(timeout);
    }

    public void AbortTransaction()
    {
        this.producer.AbortTransaction();
    }

    public void AbortTransaction(TimeSpan timeout)
    {
        producer.AbortTransaction(timeout);
    }

    public void InitTransactions(TimeSpan timeout)
    {
        producer.InitTransactions(timeout);
    }

    public void SendOffsetsToTransaction(
        IEnumerable<TopicPartitionOffset> offsets,
        IConsumerGroupMetadata groupMetadata,
        TimeSpan timeout)
    {
        producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
    }

    public int Poll(TimeSpan timeout)
    {
        return producer.Poll(timeout);
    }

    public void Flush(CancellationToken cancellationToken = default)
    {
        producer.Flush(cancellationToken);
    }

    public int Flush(TimeSpan timeout)
    {
        return producer.Flush(timeout);
    }

    public void Dispose()
    {
        producer.Dispose();
        scope.Dispose();
    }
}
