namespace Confluent.Kafka.DependencyInjection.Builders
{
    interface IBuilderAdapter<TClient>
    {
        TClient Build();
    }
}
