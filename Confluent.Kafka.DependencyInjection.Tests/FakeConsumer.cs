namespace Confluent.Kafka.DependencyInjection.Tests;

using System;
using System.Collections.Generic;
using System.Threading;

sealed class FakeConsumer(Func<TimeSpan, CancellationToken, ConsumeResult<object?, object?>?> consumeFunc) :
    IConsumer<object?, object?>
{
    public string MemberId => throw new NotImplementedException();

    public List<TopicPartition> Assignment => throw new NotImplementedException();

    public List<string> Subscription => throw new NotImplementedException();

    public IConsumerGroupMetadata ConsumerGroupMetadata => throw new NotImplementedException();

    public Handle Handle => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public int AddBrokers(string brokers)
    {
        throw new NotImplementedException();
    }

    public void Assign(TopicPartition partition)
    {
        throw new NotImplementedException();
    }

    public void Assign(TopicPartitionOffset partition)
    {
        throw new NotImplementedException();
    }

    public void Assign(IEnumerable<TopicPartitionOffset> partitions)
    {
        throw new NotImplementedException();
    }

    public void Assign(IEnumerable<TopicPartition> partitions)
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public List<TopicPartitionOffset> Commit()
    {
        throw new NotImplementedException();
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsets)
    {
        throw new NotImplementedException();
    }

    public void Commit(ConsumeResult<object?, object?> result)
    {
        throw new NotImplementedException();
    }

    public List<TopicPartitionOffset> Committed(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public ConsumeResult<object?, object?>? Consume(int millisecondsTimeout)
    {
        return consumeFunc(TimeSpan.FromMilliseconds(millisecondsTimeout), default);
    }

    public ConsumeResult<object?, object?>? Consume(CancellationToken cancellationToken = default)
    {
        return consumeFunc(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    public ConsumeResult<object?, object?>? Consume(TimeSpan timeout)
    {
        return consumeFunc(timeout, default);
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        throw new NotImplementedException();
    }

    public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
    {
        throw new NotImplementedException();
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        throw new NotImplementedException();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        throw new NotImplementedException();
    }

    public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        throw new NotImplementedException();
    }

    public Offset Position(TopicPartition partition)
    {
        throw new NotImplementedException();
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        throw new NotImplementedException();
    }

    public void Seek(TopicPartitionOffset tpo)
    {
        throw new NotImplementedException();
    }

    public void SetSaslCredentials(string username, string password)
    {
        throw new NotImplementedException();
    }

    public void StoreOffset(ConsumeResult<object?, object?> result)
    {
        throw new NotImplementedException();
    }

    public void StoreOffset(TopicPartitionOffset offset)
    {
        throw new NotImplementedException();
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        throw new NotImplementedException();
    }

    public void Subscribe(string topic)
    {
        throw new NotImplementedException();
    }

    public void Unassign()
    {
        throw new NotImplementedException();
    }

    public void Unsubscribe()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
