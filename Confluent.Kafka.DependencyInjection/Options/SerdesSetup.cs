namespace Confluent.Kafka.Options;

sealed class SerdesSetup<T>(
    ISerializer<T>? serializer = null,
    IAsyncSerializer<T>? asyncSerializer = null,
    IDeserializer<T>? deserializer = null) :
    IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (serializer != null)
        {
            if (builder is ProducerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeySerializer(serializer);
            }

            if (builder is ProducerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueSerializer(serializer);
            }
        }
        else if (asyncSerializer != null)
        {
            if (builder is ProducerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeySerializer(asyncSerializer);
            }

            if (builder is ProducerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueSerializer(asyncSerializer);
            }
        }
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        if (deserializer != null)
        {
            if (builder is ConsumerBuilder<T, TValue> keyBuilder)
            {
                keyBuilder.SetKeyDeserializer(deserializer);
            }

            if (builder is ConsumerBuilder<TKey, T> valueBuilder)
            {
                valueBuilder.SetValueDeserializer(deserializer);
            }
        }
    }

    public void Apply(AdminClientBuilder builder)
    {
    }
}
