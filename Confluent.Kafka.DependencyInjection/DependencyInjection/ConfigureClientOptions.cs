namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Options;

sealed class ConfigureClientOptions(DefaultConfigProvider config, LoggingBuilderSetup logging) :
    IConfigureNamedOptions<KafkaClientOptions>
{
    public void Configure(string? name, KafkaClientOptions options)
    {
        if (name == null || name == Options.DefaultName)
        {
            options.Configure(config);
        }

        options.Setup(logging);
    }

    public void Configure(KafkaClientOptions options)
    {
        Configure(name: null, options);
    }
}
