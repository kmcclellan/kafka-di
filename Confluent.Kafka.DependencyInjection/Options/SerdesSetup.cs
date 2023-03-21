namespace Confluent.Kafka.Options;

class SerdesSetup<T> : IClientBuilderSetup
{
    readonly ISerializer<T>? serializer;
    readonly IAsyncSerializer<T>? asyncSerializer;
    readonly IDeserializer<T>? deserializer;

    public SerdesSetup(
        ISerializer<T>? serializer = null,
        IAsyncSerializer<T>? asyncSerializer = null,
        IDeserializer<T>? deserializer = null)
    {
        this.serializer = serializer;
        this.asyncSerializer = asyncSerializer;
        this.deserializer = deserializer;
    }

    public virtual void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (this.serializer != null)
        {
            if (builder is ProducerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeySerializer(this.serializer);
            }

            if (builder is ProducerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueSerializer(this.serializer);
            }
        }
        else if (this.asyncSerializer != null)
        {
            if (builder is ProducerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeySerializer(this.asyncSerializer);
            }

            if (builder is ProducerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueSerializer(this.asyncSerializer);
            }
        }
    }

    public virtual void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        if (this.deserializer != null)
        {
            if (builder is ConsumerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeyDeserializer(this.deserializer);
            }

            if (builder is ConsumerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueDeserializer(this.deserializer);
            }
        }
    }

    public void Apply(AdminClientBuilder builder)
    {
    }
}
