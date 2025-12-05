namespace Confluent.Kafka.DependencyInjection
{
    using System.Collections.Generic;

    sealed class DefaultConfigProvider :
        IClientConfigProvider
    {
        readonly AdminClientConfig adminClientConfig;
        readonly ConsumerConfig consumerConfig;
        readonly ProducerConfig producerConfig;

        public DefaultConfigProvider(
            AdminClientConfig adminClientConfig,
            ConsumerConfig consumerConfig,
            ProducerConfig producerConfig)
        {
            this.adminClientConfig = adminClientConfig;
            this.consumerConfig = consumerConfig;
            this.producerConfig = producerConfig;
        }

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
}
