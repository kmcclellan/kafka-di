namespace Confluent.Kafka;

using Confluent.Kafka.Options;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Extensions of <see cref="IConfiguration"/> for configuring Kafka clients.
/// </summary>
public static class KafkaConfigurationExtensions
{
    /// <summary>
    /// Binds config properties for Kafka clients.
    /// </summary>
    /// <remarks>
    /// Configures producers, consumers, and admin clients using <c>Producer</c>, <c>Consumer</c>, and <c>Admin</c> sub-sections, respectively.
    /// </remarks>
    /// <param name="config">The configuration.</param>
    /// <param name="options">The Kafka client options.</param>
    public static void Bind(this IConfiguration config, KafkaClientOptions options)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (options == null) throw new ArgumentNullException(nameof(options));
#endif

        options.Configure(new StaticConfig(Get("Producer"), Get("Consumer"), Get("Admin")));

        IEnumerable<KeyValuePair<string, string>> Get(string key)
        {
            return config.GetSection(key).AsEnumerable(makePathsRelative: true)
                .Where(x => x.Value != null)!;
        }
    }
}
