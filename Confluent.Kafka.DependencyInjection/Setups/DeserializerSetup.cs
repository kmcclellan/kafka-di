namespace Confluent.Kafka.DependencyInjection.Setups;

sealed class DeserializerSetup<T> : TypedClientSetup<T>
{
    readonly IDeserializer<T> deserializer;

    public DeserializerSetup(IDeserializer<T> deserializer)
    {
        this.deserializer = deserializer;
    }
    protected override void ApplyKeys<TValue>(ConsumerBuilder<T, TValue> builder)
    {
        builder.SetKeyDeserializer(this.deserializer);
    }

    protected override void ApplyValues<TKey>(ConsumerBuilder<TKey, T> builder)
    {
        builder.SetValueDeserializer(this.deserializer);
    }
}
