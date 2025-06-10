namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;
using Confluent.Kafka.SyncOverAsync;

using Microsoft.Extensions.DependencyInjection;

sealed class ScopedConsumer<TKey, TValue> : IConsumer<TKey, TValue>
{
    static readonly Type[] BuiltInTypes = [
        typeof(Null),
        typeof(Ignore),
        typeof(int),
        typeof(long),
        typeof(string),
        typeof(float),
        typeof(double),
        typeof(byte[]),
    ];

    readonly IServiceScope scope;
    readonly IConsumer<TKey, TValue> consumer;

    bool closed;

    public ScopedConsumer(
        IServiceScopeFactory scopes,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null,
        IAsyncDeserializer<TKey>? asyncKeyDeserializer = null,
        IAsyncDeserializer<TValue>? asyncValueDeserializer = null)
    {
        scope = scopes.CreateScope();

        var config = new Dictionary<string, string>();

        foreach (var provider in scope.ServiceProvider.GetServices<IClientConfigProvider>())
        {
            var iterator = provider.ForConsumer<TKey, TValue>();

            while (iterator.MoveNext())
            {
                config[iterator.Current.Key] = iterator.Current.Value;
            }
        }

        var builder = new DIBuilder(config);

        foreach (var setup in scope.ServiceProvider.GetServices<IClientBuilderSetup>())
        {
            setup.Apply(builder);
        }

        if (builder.KeyDeserializer == null && !BuiltInTypes.Contains(typeof(TKey)))
        {
            if (keyDeserializer != null)
            {
                builder.SetKeyDeserializer(keyDeserializer);
            }
            else if (asyncKeyDeserializer != null)
            {
                builder.SetKeyDeserializer(asyncKeyDeserializer.AsSyncOverAsync());
            }
        }

        if (builder.ValueDeserializer == null && !BuiltInTypes.Contains(typeof(TValue)))
        {
            if (valueDeserializer != null)
            {
                builder.SetValueDeserializer(valueDeserializer);
            }
            else if (asyncValueDeserializer != null)
            {
                builder.SetValueDeserializer(asyncValueDeserializer.AsSyncOverAsync());
            }
        }

        consumer = builder.Build();
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

    public void SetSaslCredentials(string username, string password)
    {
        consumer.SetSaslCredentials(username, password);
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

    public TopicPartitionOffset PositionTopicPartitionOffset(TopicPartition partition)
    {
        return consumer.PositionTopicPartitionOffset(partition);
    }

    public void Close()
    {
        consumer.Close();
        closed = true;
    }

    public void Dispose()
    {
        if (!closed)
        {
            Close();
        }

        consumer.Dispose();
        scope.Dispose();
    }

    sealed class DIBuilder(IEnumerable<KeyValuePair<string, string>> config) : ConsumerBuilder<TKey, TValue>(config)
    {
        public new IDeserializer<TKey>? KeyDeserializer => base.KeyDeserializer;

        public new IDeserializer<TValue>? ValueDeserializer => base.ValueDeserializer;
    }
}
