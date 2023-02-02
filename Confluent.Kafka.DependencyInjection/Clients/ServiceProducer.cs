namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceProducer<TReceiver, TKey, TValue> : ServiceProducer<TKey, TValue>
{
    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(scopes, global?.Values.Concat(config.Values) ?? config.Values)
    {
    }
}

class ServiceProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    readonly IProducer<TKey, TValue> producer;
    readonly IServiceScope scope;

    public ServiceProducer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values)
    {
    }

    internal ServiceProducer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>> config)
    {
        scope = scopes.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<ProducerAdapter<TKey, TValue>>();

        foreach (var kvp in config)
        {
            adapter.ClientConfig[kvp.Key] = kvp.Value;
        }

        producer = adapter.Build();
    }

    public Handle Handle => producer.Handle;

    public string Name => producer.Name;

    public int AddBrokers(string brokers) =>
        producer.AddBrokers(brokers);

    public void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null) =>
        producer.Produce(topic, message, deliveryHandler);

    public void Produce(TopicPartition topicPartition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null) =>
        producer.Produce(topicPartition, message, deliveryHandler);

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(string topic, Message<TKey, TValue> message, CancellationToken cancellationToken = default) =>
        producer.ProduceAsync(topic, message, cancellationToken);

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(TopicPartition topicPartition, Message<TKey, TValue> message, CancellationToken cancellationToken = default) =>
        producer.ProduceAsync(topicPartition, message, cancellationToken);

    public void BeginTransaction() =>
        producer.BeginTransaction();

    public void CommitTransaction() =>
        this.producer.CommitTransaction();

    public void CommitTransaction(TimeSpan timeout) =>
        producer.CommitTransaction(timeout);

    public void AbortTransaction() =>
        this.producer.AbortTransaction();

    public void AbortTransaction(TimeSpan timeout) =>
        producer.AbortTransaction(timeout);

    public void InitTransactions(TimeSpan timeout) =>
        producer.InitTransactions(timeout);

    public void SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout) =>
        producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);

    public int Poll(TimeSpan timeout) =>
        producer.Poll(timeout);

    public void Flush(CancellationToken cancellationToken = default) =>
        producer.Flush(cancellationToken);

    public int Flush(TimeSpan timeout) =>
        producer.Flush(timeout);

    public void Dispose()
    {
        producer.Dispose();
        scope.Dispose();
    }
}
