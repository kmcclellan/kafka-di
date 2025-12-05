namespace Confluent.Kafka.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    sealed class ConfigureClientProperties :
        IConfigureOptions<AdminClientConfig>,
        IConfigureOptions<ConsumerConfig>,
        IConfigureOptions<ProducerConfig>
    {
        readonly IConfiguration configuration;

        public ConfigureClientProperties(IConfiguration configuration = null)
        {
            this.configuration = configuration;
        }

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
}
