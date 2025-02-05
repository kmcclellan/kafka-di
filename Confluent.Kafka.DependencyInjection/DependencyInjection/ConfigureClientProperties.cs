namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

sealed class ConfigureClientProperties(IConfiguration? config = null) :
    ConfigureOptions<KafkaClientOptions>(config != null ? x => Configure(config, x) : null)
{
    public static void Configure(IConfiguration config, KafkaClientOptions options)
    {
        config.GetSection("Kafka").Bind(options);
    }
}
