namespace Confluent.Kafka.Options;

sealed class StaticConfig : IClientConfigProvider
{
    readonly IEnumerable<KeyValuePair<string, string>> producerConfig;
    readonly IEnumerable<KeyValuePair<string, string>> consumerConfig;
    readonly IEnumerable<KeyValuePair<string, string>> adminClientConfig;

    public StaticConfig(
        IEnumerable<KeyValuePair<string, string>> producerConfig,
        IEnumerable<KeyValuePair<string, string>> consumerConfig,
        IEnumerable<KeyValuePair<string, string>> adminClientConfig)
    {
        this.producerConfig = producerConfig;
        this.consumerConfig = consumerConfig;
        this.adminClientConfig = adminClientConfig;
    }

    public IEnumerator<KeyValuePair<string, string>> ForProducer<TKey, TValue>()
    {
        return this.producerConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForConsumer<TKey, TValue>()
    {
        return this.consumerConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForAdminClient()
    {
        return this.adminClientConfig.GetEnumerator();
    }
}
