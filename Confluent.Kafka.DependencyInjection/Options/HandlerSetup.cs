namespace Confluent.Kafka.Options;

sealed class HandlerSetup : IClientBuilderSetup
{
    readonly ClientHandlers handlers;

    public HandlerSetup(ClientHandlers handlers)
    {
        this.handlers = handlers;
    }

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

        if (this.handlers.RebalanceHandler != null)
        {
            builder.SetPartitionsAssignedHandler(
                (client, partitions) =>
                {
                    var offsets = partitions.Select(x => new TopicPartitionOffset(x, Offset.Unset));
                    return this.handlers.RebalanceHandler(client, new(offsets.ToList()));
                });

            builder.SetPartitionsRevokedHandler((x, y) => this.handlers.RebalanceHandler(x, new(y, revoked: true)));
            builder.SetPartitionsLostHandler((x, y) => this.handlers.RebalanceHandler(x, new(y, lost: true)));
        }

        if (this.handlers.CommitHandler != null)
        {
            builder.SetOffsetsCommittedHandler(this.handlers.CommitHandler);
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
        if (this.handlers.ErrorHandler != null)
        {
            configureErrors(this.handlers.ErrorHandler);
        }

        if (this.handlers.StatisticsHandler != null)
        {
            configureStats(this.handlers.StatisticsHandler);
        }

        if (this.handlers.AuthenticateHandler != null)
        {
            configureAuth(this.handlers.AuthenticateHandler);
        }
    }
}
