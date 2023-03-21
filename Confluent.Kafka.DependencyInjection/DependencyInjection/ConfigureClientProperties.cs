namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

class ConfigureClientProperties : ConfigureOptions<KafkaClientOptions>
{
    public ConfigureClientProperties(IConfiguration? config = null)
        : base(config != null ? x => Configure(config, x) : null)
    {
    }

    public static void Configure(IConfiguration config, KafkaClientOptions options)
    {
        config.GetSection("Kafka").Bind(options);
    }
}
