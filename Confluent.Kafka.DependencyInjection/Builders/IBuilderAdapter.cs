using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Builders
{
    interface IBuilderAdapter<TClient>
    {
        IDictionary<string, string> ClientConfig { get; }

        TClient Build();
    }
}
