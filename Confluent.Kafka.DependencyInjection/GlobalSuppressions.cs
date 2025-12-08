using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.ConfigureClientProperties")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.DefaultConfigProvider")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.LoggingBuilderSetup")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.GlobalProducer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.GlobalConsumer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.GlobalAdminClient")]
