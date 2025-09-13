namespace Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

sealed class ConfigureClientProperties(IConfiguration? configuration = null) :
    IConfigureOptions<AdminClientConfig>,
    IConfigureOptions<ConsumerConfig>,
    IConfigureOptions<ProducerConfig>
{
    public void Configure(AdminClientConfig options)
    {
        ApplyDefaults(options);

        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }

    public void Configure(ConsumerConfig options)
    {
        ApplyDefaults(options);

        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }

    public void Configure(ProducerConfig options)
    {
        ApplyDefaults(options);

        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }

    static void ApplyDefaults(ClientConfig options)
    {
        if (string.IsNullOrEmpty(options.BootstrapServers))
        {
            options.BootstrapServers = "localhost:9092";
        }
    }
}
