namespace Tests;

using Confluent.Kafka;

sealed class FakeConsumer<TKey, TValue>
{
    readonly Queue<ConsumeSetup> setups = [];

    public FakeConsumer()
    {
        Instance = new Implementation(this);
    }

    public IConsumer<TKey, TValue> Instance { get; }

    public void Setup(ConsumeResult<TKey, TValue> result, TimeSpan minTimeout = default, Action? callback = null)
    {
        setups.Enqueue(new(minTimeout, callback, result));
    }

    public void Setup(Exception exception, TimeSpan minTimeout = default, Action? callback = null)
    {
        setups.Enqueue(new(minTimeout, callback) { Exception = exception });
    }

    ConsumeResult<TKey, TValue>? Consume(TimeSpan timeout)
    {
        if (!Monitor.TryEnter(setups))
        {
            Assert.Fail("Consumer was invoked my multiple threads concurrently.");
        }

        try
        {
            if (!setups.TryPeek(out var setup))
            {
                Assert.Fail("Consumer was invoked unexpectedly.");
            }

            if (timeout == Timeout.InfiniteTimeSpan || setup.WaitTime < timeout)
            {
                setups.Dequeue();
                setup.Callback?.Invoke();

                if (setup.Exception is not null)
                {
                    throw setup.Exception;
                }

                return setup.Result;
            }

            return null;
        }
        finally
        {
            Monitor.Exit(setups);
        }
    }

    record ConsumeSetup(
        TimeSpan WaitTime,
        Action? Callback,
        ConsumeResult<TKey, TValue>? Result = null,
        Exception? Exception = null);

    sealed class Implementation(FakeConsumer<TKey, TValue> parent) : IConsumer<TKey, TValue>
    {
        public string Name => throw new NotImplementedException();

        public string MemberId => throw new NotImplementedException();

        public IConsumerGroupMetadata ConsumerGroupMetadata => throw new NotImplementedException();

        public Handle Handle => throw new NotImplementedException();

        public List<string> Subscription => throw new NotImplementedException();

        public List<TopicPartition> Assignment => throw new NotImplementedException();

        public ConsumeResult<TKey, TValue>? Consume(int millisecondsTimeout)
        {
            return parent.Consume(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public ConsumeResult<TKey, TValue>? Consume(TimeSpan timeout)
        {
            return parent.Consume(timeout);
        }

        public ConsumeResult<TKey, TValue>? Consume(CancellationToken cancellationToken = default)
        {
            return parent.Consume(Timeout.InfiniteTimeSpan);
        }

        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
        {
            throw new NotImplementedException();
        }

        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public List<TopicPartitionOffset> OffsetsForTimes(
            IEnumerable<TopicPartitionTimestamp> timestampsToSearch,
            TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Offset Position(TopicPartition partition)
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

        public int AddBrokers(string brokers)
        {
            throw new NotImplementedException();
        }

        public void SetSaslCredentials(string username, string password)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string topic)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEnumerable<string> topics)
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

        public void Assign(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void Assign(IEnumerable<TopicPartitionOffset> partitions)
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

        public void Seek(TopicPartitionOffset tpo)
        {
            throw new NotImplementedException();
        }

        public void Pause(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void Resume(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void StoreOffset(ConsumeResult<TKey, TValue> result)
        {
            throw new NotImplementedException();
        }

        public void StoreOffset(TopicPartitionOffset offset)
        {
            throw new NotImplementedException();
        }

        public List<TopicPartitionOffset> Commit()
        {
            throw new NotImplementedException();
        }

        public void Commit(ConsumeResult<TKey, TValue> result)
        {
            throw new NotImplementedException();
        }

        public void Commit(IEnumerable<TopicPartitionOffset> offsets)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }

        public void Unassign()
        {
            throw new NotImplementedException();
        }

        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
