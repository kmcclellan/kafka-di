namespace Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.Options;

sealed class KafkaBuilderFactory
{
    readonly KafkaClientOptions options;

    public KafkaBuilderFactory(IOptionsSnapshot<KafkaClientOptions> options)
    {
        this.options = options.Value;
    }

    public ProducerBuilder<TKey, TValue> CreateProduce<TKey, TValue>()
    {
        return this.Create(x => new ProducerBuilder<TKey, TValue>(x), (x, y) => x.Apply(y));
    }

    public ConsumerBuilder<TKey, TValue> CreateConsume<TKey, TValue>()
    {
        return this.Create(x => new ConsumerBuilder<TKey, TValue>(x), (x, y) => x.Apply(y));
    }

    public AdminClientBuilder CreateAdmin()
    {
        return this.Create(x => new AdminClientBuilder(x), (x, y) => x.Apply(y));
    }

    T Create<T>(
        Func<IEnumerable<KeyValuePair<string, string>>, T> initialize,
        Action<IKafkaClientSetup, T> apply)
    {
        var builder = initialize(this.options.Properties);

        foreach (var setup in this.options.Setups)
        {
            apply(setup, builder);
        }

        return builder;
    }
}
