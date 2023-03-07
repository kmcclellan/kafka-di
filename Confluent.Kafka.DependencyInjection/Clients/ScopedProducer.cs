namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

class ScopedProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    readonly IProducer<TKey, TValue> producer;
    readonly IDisposable scope;

    public ScopedProducer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>>? config)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();

        if (config != null)
        {
            var merged = scope.ServiceProvider.GetRequiredService<ProducerConfig>();

            foreach (var kvp in config)
            {
                merged.Set(kvp.Key, kvp.Value);
            }
        }

        this.producer = scope.ServiceProvider.GetRequiredService<ProducerBuilder<TKey, TValue>>().Build();
    }

    public Handle Handle => this.producer.Handle;

    public string Name => this.producer.Name;

    public int AddBrokers(string brokers)
    {
        return this.producer.AddBrokers(brokers);
    }

    public void Produce(
        string topic,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        this.producer.Produce(topic, message, deliveryHandler);
    }

    public void Produce(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        this.producer.Produce(topicPartition, message, deliveryHandler);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
       return this.producer.ProduceAsync(topic, message, cancellationToken);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return this.producer.ProduceAsync(topicPartition, message, cancellationToken);
    }

    public void BeginTransaction()
    {
        this.producer.BeginTransaction();
    }

    public void CommitTransaction()
    {
        this.producer.CommitTransaction();
    }

    public void CommitTransaction(TimeSpan timeout)
    {
        this.producer.CommitTransaction(timeout);
    }

    public void AbortTransaction()
    {
        this.producer.AbortTransaction();
    }

    public void AbortTransaction(TimeSpan timeout)
    {
        this.producer.AbortTransaction(timeout);
    }

    public void InitTransactions(TimeSpan timeout)
    {
        this.producer.InitTransactions(timeout);
    }

    public void SendOffsetsToTransaction(
        IEnumerable<TopicPartitionOffset> offsets,
        IConsumerGroupMetadata groupMetadata,
        TimeSpan timeout)
    {
        this.producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
    }

    public int Poll(TimeSpan timeout)
    {
        return this.producer.Poll(timeout);
    }

    public void Flush(CancellationToken cancellationToken = default)
    {
        this.producer.Flush(cancellationToken);
    }

    public int Flush(TimeSpan timeout)
    {
        return this.producer.Flush(timeout);
    }

    public void Dispose()
    {
        this.producer.Dispose();
        this.scope.Dispose();
    }
}
