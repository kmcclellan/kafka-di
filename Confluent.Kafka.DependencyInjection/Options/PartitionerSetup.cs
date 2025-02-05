namespace Confluent.Kafka.Options;

sealed class PartitionerSetup(PartitionerDelegate partitioner, string? topic) : IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (topic != null)
        {
            builder.SetPartitioner(topic, partitioner);
        }
        else
        {
            builder.SetDefaultPartitioner(partitioner);
        }
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
    }

    public void Apply(AdminClientBuilder builder)
    {
    }
}
