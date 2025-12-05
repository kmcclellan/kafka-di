namespace Confluent.Kafka
{
    using Microsoft.Extensions.Configuration;

    using System;

    /// <summary>
    /// Extensions of Kafka components to integrate with <c>Microsoft.Extensions.Configuration</c>.
    /// </summary>
    public static class MSConfigurationExtensions
    {
        /// <summary>
        /// Loads properties from the <c>Kafka:Admin</c> configuration section.
        /// </summary>
        /// <param name="adminConfig">The admin client configuration instance.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void LoadFrom(this AdminClientConfig adminConfig, IConfiguration configuration)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(adminConfig, nameof(adminConfig));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
#else
            if (adminConfig == null) throw new ArgumentNullException(nameof(adminConfig));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
#endif

            LoadFrom(adminConfig, configuration, "Admin");
        }

        /// <summary>
        /// Loads properties from the <c>Kafka:Consumer</c> configuration section.
        /// </summary>
        /// <param name="consumerConfig">The consumer configuration instance.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void LoadFrom(this ConsumerConfig consumerConfig, IConfiguration configuration)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(consumerConfig, nameof(consumerConfig));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
#else
            if (consumerConfig == null) throw new ArgumentNullException(nameof(consumerConfig));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
#endif

            LoadFrom(consumerConfig, configuration, "Consumer");
        }

        /// <summary>
        /// Loads properties from the <c>Kafka:Producer</c> configuration section.
        /// </summary>
        /// <param name="producerConfig">The producer configuration instance.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void LoadFrom(this ProducerConfig producerConfig, IConfiguration configuration)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(producerConfig, nameof(producerConfig));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
#else
            if (producerConfig == null) throw new ArgumentNullException(nameof(producerConfig));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
#endif

            LoadFrom(producerConfig, configuration, "Producer");
        }

        static void LoadFrom(Config config, IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection("Kafka").GetSection(sectionName);

            foreach (var child in section.GetChildren())
            {
                if (child.Value != null)
                {
                    config.Set(child.Key, child.Value);
                }
            }
        }
    }
}
