namespace Confluent.Kafka.DependencyInjection.Clients;

using Microsoft.Extensions.DependencyInjection;

class ScopedConsumer<TKey, TValue> : IConsumer<TKey, TValue>
{
    readonly IConsumer<TKey, TValue> consumer;
    readonly IDisposable scope;

    public ScopedConsumer(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>>? config)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();

        if (config != null)
        {
            var merged = scope.ServiceProvider.GetRequiredService<ConsumerConfig>();

            foreach (var kvp in config)
            {
                merged.Set(kvp.Key, kvp.Value);
            }
        }

        this.consumer = scope.ServiceProvider.GetRequiredService<ConsumerBuilder<TKey, TValue>>().Build();
    }

    public Handle Handle => this.consumer.Handle;

    public string Name => this.consumer.Name;

    public List<string> Subscription => this.consumer.Subscription;

    public List<TopicPartition> Assignment => this.consumer.Assignment;

    public string MemberId => this.consumer.MemberId;

    public IConsumerGroupMetadata ConsumerGroupMetadata => this.consumer.ConsumerGroupMetadata;

    public int AddBrokers(string brokers)
    {
        return this.consumer.AddBrokers(brokers);
    }

    public void Subscribe(string topic)
    {
        this.consumer.Subscribe(topic);
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        this.consumer.Subscribe(topics);
    }

    public void Unsubscribe()
    {
        this.consumer.Unsubscribe();
    }

    public void Assign(TopicPartition partition)
    {
        this.consumer.Assign(partition);
    }

    public void Assign(TopicPartitionOffset partition)
    {
        this.consumer.Assign(partition);
    }

    public void Assign(IEnumerable<TopicPartitionOffset> partitions)
    {
        this.consumer.Assign(partitions);
    }

    public void Assign(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.Assign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
    {
        this.consumer.IncrementalAssign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.IncrementalAssign(partitions);
    }

    public void Unassign()
    {
        this.consumer.Unassign();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.IncrementalUnassign(partitions);
    }

    public void Seek(TopicPartitionOffset tpo)
    {
        this.consumer.Seek(tpo);
    }

    public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout)
    {
        return this.consumer.Consume(millisecondsTimeout);
    }

    public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
    {
        return this.consumer.Consume(cancellationToken);
    }

    public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
    {
        return this.consumer.Consume(timeout);
    }

    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.Pause(partitions);
    }

    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.Resume(partitions);
    }

    public List<TopicPartitionOffset> Commit()
    {
        return this.consumer.Commit();
    }

    public void Commit(ConsumeResult<TKey, TValue> result)
    {
        this.consumer.Commit(result);
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsets)
    {
        this.consumer.Commit(offsets);
    }

    public void StoreOffset(ConsumeResult<TKey, TValue> result)
    {
        this.consumer.StoreOffset(result);
    }

    public void StoreOffset(TopicPartitionOffset offset)
    {
        this.consumer.StoreOffset(offset);
    }

    public Offset Position(TopicPartition partition)
    {
        return this.consumer.Position(partition);
    }

    public List<TopicPartitionOffset> Committed(TimeSpan timeout)
    {
        return this.consumer.Committed(timeout);
    }

    public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
    {
        return this.consumer.Committed(partitions, timeout);
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        return this.consumer.GetWatermarkOffsets(topicPartition);
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        return this.consumer.QueryWatermarkOffsets(topicPartition, timeout);
    }

    public List<TopicPartitionOffset> OffsetsForTimes(
        IEnumerable<TopicPartitionTimestamp> timestampsToSearch,
        TimeSpan timeout)
    {
        return this.consumer.OffsetsForTimes(timestampsToSearch, timeout);
    }

    public virtual void Close()
    {
        this.consumer.Close();
    }

    public virtual void Dispose()
    {
        this.consumer.Dispose();
        this.scope.Dispose();
    }
}
