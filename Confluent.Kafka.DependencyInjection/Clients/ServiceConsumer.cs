using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka.DependencyInjection.Builders;

namespace Confluent.Kafka.DependencyInjection.Clients
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class ServiceConsumer<TReceiver, TKey, TValue> : ServiceConsumer<TKey, TValue>
    {
        public ServiceConsumer(ConsumerAdapter<TKey, TValue> adapter, ConfigWrapper<TReceiver> config)
            : base(adapter, config.Values) { }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class ServiceConsumer<TKey, TValue> : IConsumer<TKey, TValue>
    {
        readonly IConsumer<TKey, TValue> consumer;
        bool closed;

        public ServiceConsumer(ConsumerAdapter<TKey, TValue> adapter) : this(adapter, null) { }

        protected ServiceConsumer(
            ConsumerAdapter<TKey, TValue> adapter,
            IEnumerable<KeyValuePair<string, string>>? config)
        {
            if (config != null)
            {
                foreach (var kvp in config)
                {
                    adapter.ClientConfig[kvp.Key] = kvp.Value;
                }
            }

            consumer = adapter.Build();
        }

        public Handle Handle => consumer.Handle;

        public string Name => consumer.Name;

        public int AddBrokers(string brokers) =>
            consumer.AddBrokers(brokers);

        public List<string> Subscription => consumer.Subscription;

        public List<TopicPartition> Assignment => consumer.Assignment;

        public string MemberId => consumer.MemberId;

        public IConsumerGroupMetadata ConsumerGroupMetadata => consumer.ConsumerGroupMetadata;

        public void Subscribe(string topic) =>
            consumer.Subscribe(topic);

        public void Subscribe(IEnumerable<string> topics) =>
            consumer.Subscribe(topics);

        public void Unsubscribe() =>
            consumer.Unsubscribe();

        public void Assign(TopicPartition partition) =>
            consumer.Assign(partition);

        public void Assign(TopicPartitionOffset partition) =>
            consumer.Assign(partition);

        public void Assign(IEnumerable<TopicPartitionOffset> partitions) =>
            consumer.Assign(partitions);

        public void Assign(IEnumerable<TopicPartition> partitions) =>
            consumer.Assign(partitions);

        public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions) =>
            this.consumer.IncrementalAssign(partitions);

        public void IncrementalAssign(IEnumerable<TopicPartition> partitions) =>
            this.consumer.IncrementalAssign(partitions);

        public void Unassign() =>
            consumer.Unassign();

        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions) =>
            this.consumer.IncrementalUnassign(partitions);

        public void Seek(TopicPartitionOffset tpo) =>
            consumer.Seek(tpo);

        public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout) =>
            consumer.Consume(millisecondsTimeout);

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default) =>
            consumer.Consume(cancellationToken);

        public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout) =>
            consumer.Consume(timeout);

        public void Pause(IEnumerable<TopicPartition> partitions) =>
            consumer.Pause(partitions);

        public void Resume(IEnumerable<TopicPartition> partitions) =>
            consumer.Resume(partitions);


        public List<TopicPartitionOffset> Commit() =>
            consumer.Commit();

        public void Commit(ConsumeResult<TKey, TValue> result) =>
            consumer.Commit(result);

        public void Commit(IEnumerable<TopicPartitionOffset> offsets) =>
            consumer.Commit(offsets);

        public void StoreOffset(ConsumeResult<TKey, TValue> result) =>
            consumer.StoreOffset(result);

        public void StoreOffset(TopicPartitionOffset offset) =>
            consumer.StoreOffset(offset);

        public Offset Position(TopicPartition partition) =>
            consumer.Position(partition);

        public List<TopicPartitionOffset> Committed(TimeSpan timeout) =>
            consumer.Committed(timeout);

        public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout) =>
            consumer.Committed(partitions, timeout);

        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition) =>
            consumer.GetWatermarkOffsets(topicPartition);

        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout) =>
            consumer.QueryWatermarkOffsets(topicPartition, timeout);

        public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout) =>
            consumer.OffsetsForTimes(timestampsToSearch, timeout);

        public void Close()
        {
            consumer.Close();
            closed = true;
        }

        public void Dispose()
        {
            // Close when disposed by ServiceProvider.
            if (!closed) consumer.Close();
            consumer.Dispose();
        }
    }
}
