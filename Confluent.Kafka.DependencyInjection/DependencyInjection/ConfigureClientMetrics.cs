namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.DependencyInjection.Statistics;
using Confluent.Kafka.Options;

using Microsoft.Extensions.Options;

using System.Diagnostics.Metrics;

sealed class ConfigureClientMetrics : ConfigureNamedOptions<KafkaClientOptions>
{
    public ConfigureClientMetrics()
        : base(null, Configure)
    {
    }

    private static new void Configure(KafkaClientOptions options)
    {
        KafkaClientOptions.Metrics.CreateObservableCounter(
            "client-messages",
            () => GetMessageCounts(options.CaptureStatistics().Values),
            "events",
            "Total number of Kafka events produced or consumed by a client.");
    }

    static IEnumerable<Measurement<long>> GetMessageCounts(IEnumerable<KafkaStatistics> stats)
    {
        foreach (var item in stats)
        {
            var tags = new KeyValuePair<string, object?>[]
            {
                new("client-type", item.Type),
                new("client-name", item.Name),
            };

            switch (item.Type)
            {
                case "producer":
                    yield return new(item.TransmittedMessages, tags);
                    break;

                case "consumer":
                    yield return new(item.ReceivedMessages, tags);
                    break;
            }
        }
    }
}
