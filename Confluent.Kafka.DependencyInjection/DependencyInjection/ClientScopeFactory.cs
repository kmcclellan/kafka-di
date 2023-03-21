namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Options;

delegate IDisposable ClientScopeFactory(out KafkaClientOptions options);
