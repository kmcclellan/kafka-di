namespace Confluent.Kafka.Options;

sealed class PartitionerSetup : IClientBuilderSetup
{
    readonly PartitionerDelegate partitioner;
    readonly string? topic;

    public PartitionerSetup(PartitionerDelegate partitioner, string? topic)
    {
        this.partitioner = partitioner;
        this.topic = topic;
    }

    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        if (this.topic != null)
        {
            builder.SetPartitioner(this.topic, this.partitioner);
        }
        else
        {
            builder.SetDefaultPartitioner(this.partitioner);
        }
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
    }

    public void Apply(AdminClientBuilder builder)
    {
    }
}
