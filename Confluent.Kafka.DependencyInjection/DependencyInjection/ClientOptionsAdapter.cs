namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Options;

using System.Collections.Generic;

sealed class ClientOptionsAdapter(
    IOptionsSnapshot<KafkaClientOptions> options) :
    IClientConfigProvider,
    IClientBuilderSetup
{
    public IEnumerator<KeyValuePair<string, string>> ForAdminClient()
    {
        return GetConfig(x => x.ForAdminClient());
    }

    public IEnumerator<KeyValuePair<string, string>> ForConsumer<TKey, TValue>()
    {
        return GetConfig(x => x.ForConsumer<TKey, TValue>());
    }

    public IEnumerator<KeyValuePair<string, string>> ForProducer<TKey, TValue>()
    {
        return GetConfig(x => x.ForProducer<TKey, TValue>());
    }

    public void Apply(AdminClientBuilder builder)
    {
        foreach (var setup in options.Value.BuilderSetups)
        {
            setup.Apply(builder);
        }
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {

        foreach (var setup in options.Value.BuilderSetups)
        {
            setup.Apply(builder);
        }
    }

    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {

        foreach (var setup in options.Value.BuilderSetups)
        {
            setup.Apply(builder);
        }
    }

    IEnumerator<KeyValuePair<string, string>> GetConfig(
        Func<IClientConfigProvider, IEnumerator<KeyValuePair<string, string>>> selector)
    {
        foreach (var provider in options.Value.ConfigProviders)
        {
            var iterator = selector(provider);

            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }
        }
    }
}
