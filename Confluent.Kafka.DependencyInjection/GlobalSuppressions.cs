
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.ScopedProducer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.ScopedConsumer`2")]

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Scope = "type",
    Target = "~T:Confluent.Kafka.DependencyInjection.ScopedAdminClient")]
