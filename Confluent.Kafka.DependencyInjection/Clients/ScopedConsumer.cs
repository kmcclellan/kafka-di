namespace Confluent.Kafka.DependencyInjection.Clients;

using System;
using System.Collections.Generic;
using System.Threading;

class ScopedConsumer<TKey, TValue> : IConsumer<TKey, TValue>
{
    readonly IConsumer<TKey, TValue> consumer;
    readonly IDisposable scope;

    public ScopedConsumer(IConsumer<TKey, TValue> consumer, IDisposable scope)
    {
        this.consumer = consumer;
        this.scope = scope;
    }

    public Handle Handle => consumer.Handle;

    public string Name => consumer.Name;

    public List<string> Subscription => consumer.Subscription;

    public List<TopicPartition> Assignment => consumer.Assignment;

    public string MemberId => consumer.MemberId;

    public IConsumerGroupMetadata ConsumerGroupMetadata => consumer.ConsumerGroupMetadata;

    public int AddBrokers(string brokers)
    {
        return consumer.AddBrokers(brokers);
    }

    public void Subscribe(string topic)
    {
        consumer.Subscribe(topic);
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        consumer.Subscribe(topics);
    }

    public void Unsubscribe()
    {
        consumer.Unsubscribe();
    }

    public void Assign(TopicPartition partition)
    {
        consumer.Assign(partition);
    }

    public void Assign(TopicPartitionOffset partition)
    {
        consumer.Assign(partition);
    }

    public void Assign(IEnumerable<TopicPartitionOffset> partitions)
    {
        consumer.Assign(partitions);
    }

    public void Assign(IEnumerable<TopicPartition> partitions)
    {
        consumer.Assign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
    {
        consumer.IncrementalAssign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        consumer.IncrementalAssign(partitions);
    }

    public void Unassign()
    {
        consumer.Unassign();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        consumer.IncrementalUnassign(partitions);
    }

    public void Seek(TopicPartitionOffset tpo)
    {
        consumer.Seek(tpo);
    }

    public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout)
    {
        return consumer.Consume(millisecondsTimeout);
    }

    public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
    {
        return consumer.Consume(cancellationToken);
    }

    public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
    {
        return consumer.Consume(timeout);
    }

    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        consumer.Pause(partitions);
    }

    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        consumer.Resume(partitions);
    }

    public List<TopicPartitionOffset> Commit()
    {
        return consumer.Commit();
    }

    public void Commit(ConsumeResult<TKey, TValue> result)
    {
        consumer.Commit(result);
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsets)
    {
        consumer.Commit(offsets);
    }

    public void StoreOffset(ConsumeResult<TKey, TValue> result)
    {
        consumer.StoreOffset(result);
    }

    public void StoreOffset(TopicPartitionOffset offset)
    {
        consumer.StoreOffset(offset);
    }

    public Offset Position(TopicPartition partition)
    {
        return consumer.Position(partition);
    }

    public List<TopicPartitionOffset> Committed(TimeSpan timeout)
    {
        return consumer.Committed(timeout);
    }

    public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
    {
        return consumer.Committed(partitions, timeout);
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        return consumer.GetWatermarkOffsets(topicPartition);
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        return consumer.QueryWatermarkOffsets(topicPartition, timeout);
    }

    public List<TopicPartitionOffset> OffsetsForTimes(
        IEnumerable<TopicPartitionTimestamp> timestampsToSearch,
        TimeSpan timeout)
    {
        return consumer.OffsetsForTimes(timestampsToSearch, timeout);
    }

    public virtual void Close()
    {
        consumer.Close();
    }

    public virtual void Dispose()
    {
        consumer.Dispose();
        scope.Dispose();
    }
}
