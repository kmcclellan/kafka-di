namespace Confluent.Kafka.DependencyInjection.Setups;

abstract class TypedClientSetup<T> : IKafkaClientSetup
{
    public virtual void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (builder is ProducerBuilder<T, TValue> keyBuilder)
        {
            this.ApplyKeys(keyBuilder);
        }

        if (builder is ProducerBuilder<TKey, T> valueBuilder)
        {
            this.ApplyValues(valueBuilder);
        }
    }

    public virtual void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        if (builder is ConsumerBuilder<T, TValue> keyBuilder)
        {
            this.ApplyKeys(keyBuilder);
        }

        if (builder is ConsumerBuilder<TKey, T> valueBuilder)
        {
            this.ApplyValues(valueBuilder);
        }
    }

    public virtual void Apply(AdminClientBuilder builder)
    {
    }

    protected virtual void ApplyKeys<TValue>(ProducerBuilder<T, TValue> builder)
    {
    }

    protected virtual void ApplyKeys<TValue>(ConsumerBuilder<T, TValue> builder)
    {
    }

    protected virtual void ApplyValues<TKey>(ProducerBuilder<TKey, T> builder)
    {
    }

    protected virtual void ApplyValues<TKey>(ConsumerBuilder<TKey, T> builder)
    {
    }
}
