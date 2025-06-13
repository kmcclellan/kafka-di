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
        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }

    public void Configure(ConsumerConfig options)
    {
        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }

    public void Configure(ProducerConfig options)
    {
        if (configuration != null)
        {
            options.LoadFrom(configuration);
        }
    }
}
