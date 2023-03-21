namespace Confluent.Kafka.DependencyInjection;

sealed class ScopedConsumer<TKey, TValue> : IConsumer<TKey, TValue>
{
    readonly IDisposable scope;
    readonly IConsumer<TKey, TValue> inner;

    bool closed;

    public ScopedConsumer(ClientScopeFactory initOptions)
    {
        this.scope = initOptions(out var options);
        this.inner = options.CreateConsumer<TKey, TValue>();
    }

    public Handle Handle => this.inner.Handle;

    public string Name => this.inner.Name;

    public List<string> Subscription => this.inner.Subscription;

    public List<TopicPartition> Assignment => this.inner.Assignment;

    public string MemberId => this.inner.MemberId;

    public IConsumerGroupMetadata ConsumerGroupMetadata => this.inner.ConsumerGroupMetadata;

    public int AddBrokers(string brokers)
    {
        return this.inner.AddBrokers(brokers);
    }

    public void Subscribe(string topic)
    {
        this.inner.Subscribe(topic);
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        this.inner.Subscribe(topics);
    }

    public void Unsubscribe()
    {
        this.inner.Unsubscribe();
    }

    public void Assign(TopicPartition partition)
    {
        this.inner.Assign(partition);
    }

    public void Assign(TopicPartitionOffset partition)
    {
        this.inner.Assign(partition);
    }

    public void Assign(IEnumerable<TopicPartitionOffset> partitions)
    {
        this.inner.Assign(partitions);
    }

    public void Assign(IEnumerable<TopicPartition> partitions)
    {
        this.inner.Assign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
    {
        this.inner.IncrementalAssign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        this.inner.IncrementalAssign(partitions);
    }

    public void Unassign()
    {
        this.inner.Unassign();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        this.inner.IncrementalUnassign(partitions);
    }

    public void Seek(TopicPartitionOffset tpo)
    {
        this.inner.Seek(tpo);
    }

    public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout)
    {
        return this.inner.Consume(millisecondsTimeout);
    }

    public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
    {
        return this.inner.Consume(cancellationToken);
    }

    public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
    {
        return this.inner.Consume(timeout);
    }

    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        this.inner.Pause(partitions);
    }

    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        this.inner.Resume(partitions);
    }

    public List<TopicPartitionOffset> Commit()
    {
        return this.inner.Commit();
    }

    public void Commit(ConsumeResult<TKey, TValue> result)
    {
        this.inner.Commit(result);
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsets)
    {
        this.inner.Commit(offsets);
    }

    public void StoreOffset(ConsumeResult<TKey, TValue> result)
    {
        this.inner.StoreOffset(result);
    }

    public void StoreOffset(TopicPartitionOffset offset)
    {
        this.inner.StoreOffset(offset);
    }

    public Offset Position(TopicPartition partition)
    {
        return this.inner.Position(partition);
    }

    public List<TopicPartitionOffset> Committed(TimeSpan timeout)
    {
        return this.inner.Committed(timeout);
    }

    public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
    {
        return this.inner.Committed(partitions, timeout);
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        return this.inner.GetWatermarkOffsets(topicPartition);
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        return this.inner.QueryWatermarkOffsets(topicPartition, timeout);
    }

    public List<TopicPartitionOffset> OffsetsForTimes(
        IEnumerable<TopicPartitionTimestamp> timestampsToSearch,
        TimeSpan timeout)
    {
        return this.inner.OffsetsForTimes(timestampsToSearch, timeout);
    }

    public void Close()
    {
        this.inner.Close();
        this.closed = true;
    }

    public void Dispose()
    {
        if (!this.closed)
        {
            this.Close();
        }

        this.inner.Dispose();
        this.scope.Dispose();
    }
}
