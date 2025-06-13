namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using System.Collections.Generic;

sealed class DefaultConfigProvider(
    AdminClientConfig adminClientConfig,
    ConsumerConfig consumerConfig,
    ProducerConfig producerConfig) :
    IClientConfigProvider
{
    public IEnumerator<KeyValuePair<string, string>> ForAdminClient()
    {
        return adminClientConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForConsumer<TKey, TValue>()
    {
        return consumerConfig.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> ForProducer<TKey, TValue>()
    {
        return producerConfig.GetEnumerator();
    }
}
