namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.DependencyInjection;

sealed class ScopedProducer<TKey, TValue> : IProducer<TKey, TValue>
{
    readonly IServiceScope scope;
    readonly IProducer<TKey, TValue> producer;

    public ScopedProducer(IServiceScopeFactory scopes)
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

        var builder = new ProducerBuilder<TKey, TValue>(config);

        foreach (var setup in scope.ServiceProvider.GetServices<IClientBuilderSetup>())
        {
            setup.Apply(builder);
        }

        producer = builder.Build();
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
        producer.Produce(topic, message, deliveryHandler);
    }

    public void Produce(
        TopicPartition topicPartition,
        Message<TKey, TValue> message,
        Action<DeliveryReport<TKey, TValue>>? deliveryHandler = null)
    {
        producer.Produce(topicPartition, message, deliveryHandler);
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
        producer.Dispose();
        scope.Dispose();
    }
}
