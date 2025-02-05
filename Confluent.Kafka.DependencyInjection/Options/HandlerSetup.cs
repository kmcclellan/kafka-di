namespace Confluent.Kafka.Options;

sealed class HandlerSetup(ClientHandlers handlers) : IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        this.ApplyHandlers(
            builder.SetErrorHandler,
            builder.SetStatisticsHandler,
            builder.SetOAuthBearerTokenRefreshHandler);
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        this.ApplyHandlers(
            builder.SetErrorHandler,
            builder.SetStatisticsHandler,
            builder.SetOAuthBearerTokenRefreshHandler);

        if (handlers.RebalanceHandler != null)
        {
            builder.SetPartitionsAssignedHandler(
                (client, partitions) =>
                {
                    var offsets = partitions.Select(x => new TopicPartitionOffset(x, Offset.Unset));
                    return handlers.RebalanceHandler(client, new(offsets.ToList()));
                });

            builder.SetPartitionsRevokedHandler((x, y) => handlers.RebalanceHandler(x, new(y, revoked: true)));
            builder.SetPartitionsLostHandler((x, y) => handlers.RebalanceHandler(x, new(y, lost: true)));
        }

        if (handlers.CommitHandler != null)
        {
            builder.SetOffsetsCommittedHandler(handlers.CommitHandler);
        }
    }

    public void Apply(AdminClientBuilder builder)
    {
        this.ApplyHandlers(
            builder.SetErrorHandler,
            builder.SetStatisticsHandler,
            builder.SetOAuthBearerTokenRefreshHandler);
    }

    void ApplyHandlers<TBuilder>(
        Func<Action<IClient, Error>, TBuilder> configureErrors,
        Func<Action<IClient, string>, TBuilder> configureStats,
        Func<Action<IClient, string>, TBuilder> configureAuth)
    {
        if (handlers.ErrorHandler != null)
        {
            configureErrors(handlers.ErrorHandler);
        }

        if (handlers.StatisticsHandler != null)
        {
            configureStats(handlers.StatisticsHandler);
        }

        if (handlers.AuthenticateHandler != null)
        {
            configureAuth(handlers.AuthenticateHandler);
        }
    }
}
