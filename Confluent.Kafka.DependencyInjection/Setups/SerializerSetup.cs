namespace Confluent.Kafka.DependencyInjection.Setups;

sealed class SerializerSetup<T> : TypedClientSetup<T>
{
    readonly ISerializer<T> serializer;

    public SerializerSetup(ISerializer<T> serializer)
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
