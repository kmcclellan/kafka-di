using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.DIProducer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.DIConsumer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.DIAdminClient")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.KafkaBuilderFactory")]
