namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.DependencyInjection.Handlers;

sealed class DIAdminClientBuilder : AdminClientBuilder
{
    readonly IEnumerable<IErrorHandler> errorHandlers;
    readonly IEnumerable<IStatisticsHandler> statisticsHandlers;
    readonly IEnumerable<ILogHandler> logHandlers;

    public DIAdminClientBuilder(
        AdminClientConfig config,
        IEnumerable<IErrorHandler> errorHandlers,
        IEnumerable<IStatisticsHandler> statisticsHandlers,
        IEnumerable<ILogHandler> logHandlers)
        : base(config)
    {
        this.errorHandlers = errorHandlers;
        this.statisticsHandlers = statisticsHandlers;
        this.logHandlers = logHandlers;
    }

    public override IAdminClient Build()
    {
        this.ErrorHandler ??= this.errorHandlers.Aggregate(default(Action<IClient, Error>), (x, y) => x + y.OnError);

        this.StatisticsHandler ??= this.statisticsHandlers.Aggregate(
            default(Action<IClient, string>),
            (x, y) => x + y.OnStatistics);

        this.LogHandler ??= this.logHandlers.Aggregate(default(Action<IClient, LogMessage>), (x, y) => x + y.OnLog);

        return base.Build();
    }
}
