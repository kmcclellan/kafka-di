namespace Confluent.Kafka.DependencyInjection.Builders;

using System.Collections.Generic;

interface IBuilderAdapter<TClient>
{
    IDictionary<string, string> ClientConfig { get; }

    TClient Build();
}
