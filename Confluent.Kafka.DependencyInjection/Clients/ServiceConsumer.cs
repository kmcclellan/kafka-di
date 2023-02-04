namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Builders;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
sealed class ServiceConsumer<TReceiver, TKey, TValue> : ServiceConsumer<TKey, TValue>
{
    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper<TReceiver> config, ConfigWrapper? global = null)
        : base(scopes, global?.Values.Concat(config.Values) ?? config.Values)
    {
    }
}

class ServiceConsumer<TKey, TValue> : IConsumer<TKey, TValue>
{
    readonly IConsumer<TKey, TValue> consumer;
    readonly IDisposable scope;
    readonly bool closeOnDispose;

    bool closed;

    public ServiceConsumer(IServiceScopeFactory scopes, ConfigWrapper config)
        : this(scopes, config.Values, closeOnDispose: true)
    {
    }

    internal ServiceConsumer(
        IServiceScopeFactory scopes,
        IEnumerable<KeyValuePair<string, string>> config,
        bool closeOnDispose = false)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();
        consumer = new ConsumerAdapter<TKey, TValue>(config, scope, dispose: false).Build();
        this.closeOnDispose = closeOnDispose;
    }

    internal ServiceConsumer(IConsumer<TKey, TValue> consumer, IDisposable scope)
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
        this.consumer.IncrementalAssign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.IncrementalAssign(partitions);
    }

    public void Unassign()
    {
        consumer.Unassign();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        this.consumer.IncrementalUnassign(partitions);
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

    public void Close()
    {
        consumer.Close();
        closed = true;
    }

    public void Dispose()
    {
        if (closeOnDispose && !closed)
        {
            // Close when disposed by ServiceProvider.
            consumer.Close();
        }

        consumer.Dispose();
        scope.Dispose();
    }
}
