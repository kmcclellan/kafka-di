namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.SyncOverAsync;

using Microsoft.Extensions.DependencyInjection;

sealed class ScopedConsumer<TKey, TValue>(
    IServiceScopeFactory scopes,
    IDeserializer<TKey>? keyDeserializer = null,
    IDeserializer<TValue>? valueDeserializer = null,
    IAsyncDeserializer<TKey>? asyncKeyDeserializer = null,
    IAsyncDeserializer<TValue>? asyncValueDeserializer = null) :
    IConsumer<TKey, TValue>
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

    readonly object syncObj = new();

    IServiceScope? scope;
    IConsumer<TKey, TValue>? consumer;

    bool closed;

    public Handle Handle => Consumer.Handle;

    public string Name => Consumer.Name;

    public List<string> Subscription => Consumer.Subscription;

    public List<TopicPartition> Assignment => Consumer.Assignment;

    public string MemberId => Consumer.MemberId;

    public IConsumerGroupMetadata ConsumerGroupMetadata => Consumer.ConsumerGroupMetadata;

    IConsumer<TKey, TValue> Consumer
    {
        get
        {
            if (consumer == null)
            {
                lock (syncObj)
                {
                    if (consumer == null)
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
                }
            }

            return consumer;
        }
    }

    public int AddBrokers(string brokers)
    {
        return Consumer.AddBrokers(brokers);
    }

    public void SetSaslCredentials(string username, string password)
    {
        Consumer.SetSaslCredentials(username, password);
    }

    public void Subscribe(string topic)
    {
        Consumer.Subscribe(topic);
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        Consumer.Subscribe(topics);
    }

    public void Unsubscribe()
    {
        Consumer.Unsubscribe();
    }

    public void Assign(TopicPartition partition)
    {
        Consumer.Assign(partition);
    }

    public void Assign(TopicPartitionOffset partition)
    {
        Consumer.Assign(partition);
    }

    public void Assign(IEnumerable<TopicPartitionOffset> partitions)
    {
        Consumer.Assign(partitions);
    }

    public void Assign(IEnumerable<TopicPartition> partitions)
    {
        Consumer.Assign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
    {
        Consumer.IncrementalAssign(partitions);
    }

    public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
    {
        Consumer.IncrementalAssign(partitions);
    }

    public void Unassign()
    {
        Consumer.Unassign();
    }

    public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
    {
        Consumer.IncrementalUnassign(partitions);
    }

    public void Seek(TopicPartitionOffset tpo)
    {
        Consumer.Seek(tpo);
    }

    public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout)
    {
        return Consumer.Consume(millisecondsTimeout);
    }

    public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
    {
        return Consumer.Consume(cancellationToken);
    }

    public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
    {
        return Consumer.Consume(timeout);
    }

    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        Consumer.Pause(partitions);
    }

    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        Consumer.Resume(partitions);
    }

    public List<TopicPartitionOffset> Commit()
    {
        return Consumer.Commit();
    }

    public void Commit(ConsumeResult<TKey, TValue> result)
    {
        Consumer.Commit(result);
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsets)
    {
        Consumer.Commit(offsets);
    }

    public void StoreOffset(ConsumeResult<TKey, TValue> result)
    {
        Consumer.StoreOffset(result);
    }

    public void StoreOffset(TopicPartitionOffset offset)
    {
        Consumer.StoreOffset(offset);
    }

    public Offset Position(TopicPartition partition)
    {
        return Consumer.Position(partition);
    }

    public List<TopicPartitionOffset> Committed(TimeSpan timeout)
    {
        return Consumer.Committed(timeout);
    }

    public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
    {
        return Consumer.Committed(partitions, timeout);
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        return Consumer.GetWatermarkOffsets(topicPartition);
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        return Consumer.QueryWatermarkOffsets(topicPartition, timeout);
    }

    public List<TopicPartitionOffset> OffsetsForTimes(
        IEnumerable<TopicPartitionTimestamp> timestampsToSearch,
        TimeSpan timeout)
    {
        return Consumer.OffsetsForTimes(timestampsToSearch, timeout);
    }

    public TopicPartitionOffset PositionTopicPartitionOffset(TopicPartition partition)
    {
        return Consumer.PositionTopicPartitionOffset(partition);
    }

    public void Close()
    {
        if (consumer != null)
        {
            consumer.Close();
            closed = true;
        }
    }

    public void Dispose()
    {
        if (!closed)
        {
            Close();
        }

        consumer?.Dispose();
        scope?.Dispose();
    }

    sealed class DIBuilder(IEnumerable<KeyValuePair<string, string>> config) : ConsumerBuilder<TKey, TValue>(config)
    {
        public new IDeserializer<TKey>? KeyDeserializer => base.KeyDeserializer;

        public new IDeserializer<TValue>? ValueDeserializer => base.ValueDeserializer;
    }
}
