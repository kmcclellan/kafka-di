namespace Confluent.Kafka.DependencyInjection;

sealed class ScopedProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    readonly IDisposable scope;
    readonly IProducer<TKey, TValue> inner;

    public ScopedProducer(ClientScopeFactory initOptions)
    {
        this.scope = initOptions(out var options);
        this.inner = options.CreateProducer<TKey, TValue>();
    }

    public Handle Handle => this.inner.Handle;

    public string Name => this.inner.Name;

    public int AddBrokers(string brokers)
    {
        return this.inner.AddBrokers(brokers);
    }

    public void Produce(
        string topic,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        this.inner.Produce(topic, message, deliveryHandler);
    }

    public void Produce(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        this.inner.Produce(topicPartition, message, deliveryHandler);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return this.inner.ProduceAsync(topic, message, cancellationToken);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return this.inner.ProduceAsync(topicPartition, message, cancellationToken);
    }

    public void BeginTransaction()
    {
        this.inner.BeginTransaction();
    }

    public void CommitTransaction()
    {
        this.inner.CommitTransaction();
    }

    public void CommitTransaction(TimeSpan timeout)
    {
        this.inner.CommitTransaction(timeout);
    }

    public void AbortTransaction()
    {
        this.inner.AbortTransaction();
    }

    public void AbortTransaction(TimeSpan timeout)
    {
        this.inner.AbortTransaction(timeout);
    }

    public void InitTransactions(TimeSpan timeout)
    {
        this.inner.InitTransactions(timeout);
    }

    public void SendOffsetsToTransaction(
        IEnumerable<TopicPartitionOffset> offsets,
        IConsumerGroupMetadata groupMetadata,
        TimeSpan timeout)
    {
        this.inner.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
    }

    public int Poll(TimeSpan timeout)
    {
        return this.inner.Poll(timeout);
    }

    public void Flush(CancellationToken cancellationToken = default)
    {
        this.inner.Flush(cancellationToken);
    }

    public int Flush(TimeSpan timeout)
    {
        return this.inner.Flush(timeout);
    }

    public void Dispose()
    {
        this.inner.Dispose();
        this.scope.Dispose();
    }
}
