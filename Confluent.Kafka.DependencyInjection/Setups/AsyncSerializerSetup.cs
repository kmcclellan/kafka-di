namespace Confluent.Kafka.DependencyInjection.Setups;

sealed class AsyncSerializerSetup<T> : TypedClientSetup<T>
{
    readonly IAsyncSerializer<T> serializer;

    public AsyncSerializerSetup(IAsyncSerializer<T> serializer)
    {
        this.serializer = serializer;
    }

    protected override void ApplyKeys<TValue>(ProducerBuilder<T, TValue> builder)
    {
        builder.SetKeySerializer(this.serializer);
    }

    protected override void ApplyValues<TKey>(ProducerBuilder<TKey, T> builder)
    {
        builder.SetValueSerializer(this.serializer);
    }
}
