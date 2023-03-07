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
        return this.Create(
            x => new ProducerBuilder<TKey, TValue>(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y));
    }

    public ConsumerBuilder<TKey, TValue> CreateConsume<TKey, TValue>()
    {
        return this.Create(
            x => new ConsumerBuilder<TKey, TValue>(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y));
    }

    public AdminClientBuilder CreateAdmin()
    {
        return this.Create(
            x => new AdminClientBuilder(x),
            (x, y) => x.Apply(y),
            (x, y) => x.SetErrorHandler(y),
            (x, y) => x.SetStatisticsHandler(y),
            (x, y) => x.SetOAuthBearerTokenRefreshHandler(y));
    }

    T Create<T>(
        Func<IEnumerable<KeyValuePair<string, string>>, T> initialize,
        Action<IKafkaClientSetup, T> apply,
        Action<T, Action<IClient, Error>> configureErrors,
        Action<T, Action<IClient, string>> configureStats,
        Action<T, Action<IClient, string>> configureAuth)
    {
        var builder = initialize(this.options.Properties);

        foreach (var setup in this.options.Setups)
        {
            apply(setup, builder);
        }

        if (this.options.ErrorHandler != null)
        {
            configureErrors(builder, this.options.ErrorHandler);
        }

        if (this.options.StatisticsHandler != null)
        {
            configureStats(builder, this.options.StatisticsHandler);
        }

        if (this.options.AuthenticateHandler != null)
        {
            configureAuth(builder, this.options.AuthenticateHandler);
        }

        return builder;
    }
}
