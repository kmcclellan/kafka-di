namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;
using Confluent.Kafka.SyncOverAsync;

using Microsoft.Extensions.DependencyInjection;

sealed class ScopedProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    static readonly Type[] BuiltInTypes = [
        typeof(Null),
        typeof(int),
        typeof(long),
        typeof(string),
        typeof(float),
        typeof(double),
        typeof(byte[]),
    ];

    readonly IServiceScope scope;
    readonly IProducer<TKey, TValue> producer;
    readonly IProducer<TKey, TValue>? syncProducer;

    public ScopedProducer(
        IServiceScopeFactory scopes,
        ISerializer<TKey>? keySerializer = null,
        ISerializer<TValue>? valueSerializer = null,
        IAsyncSerializer<TKey>? asyncKeySerializer = null,
        IAsyncSerializer<TValue>? asyncValueSerializer = null)
    {
        scope = scopes.CreateScope();

        var config = new Dictionary<string, string>();

        foreach (var provider in scope.ServiceProvider.GetServices<IClientConfigProvider>())
        {
            var iterator = provider.ForProducer<TKey, TValue>();

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

        if (builder.KeySerializer == null && builder.AsyncKeySerializer == null &&
            !BuiltInTypes.Contains(typeof(TKey)))
        {
            if (keySerializer != null)
            {
                builder.SetKeySerializer(keySerializer);
            }
            else if (asyncKeySerializer != null)
            {
                builder.SetKeySerializer(asyncKeySerializer);
            }
        }

        if (builder.ValueSerializer == null && builder.AsyncValueSerializer == null &&
            !BuiltInTypes.Contains(typeof(TValue)))
        {
            if (valueSerializer != null)
            {
                builder.SetValueSerializer(valueSerializer);
            }
            else if (asyncValueSerializer != null)
            {
                builder.SetValueSerializer(asyncValueSerializer);
            }
        }

        producer = builder.Build();

        if (builder.AsyncKeySerializer != null || builder.AsyncValueSerializer != null)
        {
            // Workaround to support both modes of delivery handling.
            // https://github.com/confluentinc/confluent-kafka-dotnet/issues/2481
            var syncBuilder = new DependentProducerBuilder<TKey, TValue>(producer.Handle);

            if (builder.AsyncKeySerializer != null)
            {
                syncBuilder.SetKeySerializer(builder.AsyncKeySerializer.AsSyncOverAsync());
            }
            else if (builder.KeySerializer != null)
            {
                syncBuilder.SetKeySerializer(builder.KeySerializer);
            }

            if (builder.AsyncValueSerializer != null)
            {
                syncBuilder.SetValueSerializer(builder.AsyncValueSerializer.AsSyncOverAsync());
            }
            else if (builder.ValueSerializer != null)
            {
                syncBuilder.SetValueSerializer(builder.ValueSerializer);
            }

            syncProducer = syncBuilder.Build();
        }
    }

    public Handle Handle => producer.Handle;

    public string Name => producer.Name;

    public int AddBrokers(string brokers)
    {
        return producer.AddBrokers(brokers);
    }

    public void SetSaslCredentials(string username, string password)
    {
        producer.SetSaslCredentials(username, password);
    }

    public void Produce(
        string topic,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        (syncProducer ?? producer).Produce(topic, message, deliveryHandler);
    }

    public void Produce(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        (syncProducer ?? producer).Produce(topicPartition, message, deliveryHandler);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return producer.ProduceAsync(topic, message, cancellationToken);
    }

    public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        return producer.ProduceAsync(topicPartition, message, cancellationToken);
    }

    public void BeginTransaction()
    {
        producer.BeginTransaction();
    }

    public void CommitTransaction()
    {
        producer.CommitTransaction();
    }

    public void CommitTransaction(TimeSpan timeout)
    {
        producer.CommitTransaction(timeout);
    }

    public void AbortTransaction()
    {
        producer.AbortTransaction();
    }

    public void AbortTransaction(TimeSpan timeout)
    {
        producer.AbortTransaction(timeout);
    }

    public void InitTransactions(TimeSpan timeout)
    {
        producer.InitTransactions(timeout);
    }

    public void SendOffsetsToTransaction(
        IEnumerable<TopicPartitionOffset> offsets,
        IConsumerGroupMetadata groupMetadata,
        TimeSpan timeout)
    {
        producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
    }

    public int Poll(TimeSpan timeout)
    {
        return producer.Poll(timeout);
    }

    public void Flush(CancellationToken cancellationToken = default)
    {
        producer.Flush(cancellationToken);
    }

    public int Flush(TimeSpan timeout)
    {
        return producer.Flush(timeout);
    }

    public void Dispose()
    {
        syncProducer?.Dispose();
        producer.Dispose();
        scope.Dispose();
    }

    sealed class DIBuilder(IEnumerable<KeyValuePair<string, string>> config) : ProducerBuilder<TKey, TValue>(config)
    {
        public new ISerializer<TKey>? KeySerializer => base.KeySerializer;

        public new ISerializer<TValue>? ValueSerializer => base.ValueSerializer;

        public new IAsyncSerializer<TKey>? AsyncKeySerializer => base.AsyncKeySerializer;

        public new IAsyncSerializer<TValue>? AsyncValueSerializer => base.AsyncValueSerializer;
    }
}
