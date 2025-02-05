namespace Confluent.Kafka.Options;

sealed class StaticConfig(
    IEnumerable<KeyValuePair<string, string>> producerConfig,
    IEnumerable<KeyValuePair<string, string>> consumerConfig,
    IEnumerable<KeyValuePair<string, string>> adminClientConfig) :
    IClientConfigProvider
{
    public IEnumerator<KeyValuePair<string, string>> ForProducer<TKey, TValue>()
    {
        return producerConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForConsumer<TKey, TValue>()
    {
        return consumerConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForAdminClient()
    {
        return adminClientConfig.GetEnumerator();
    }
}
