using Confluent.Kafka;
using Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

await using var provider = new ServiceCollection()
    .AddKafkaClient(
        opts =>
        {
            opts.Properties["client.id"] = "global";
            opts.Properties["bootstrap.servers"] = "localhost:9092";
            opts.Properties["enable.idempotence"] = "true";
            opts.Properties["group.id"] = "group1";
        })
    .BuildServiceProvider();

var producer = provider.GetRequiredService<IProducer<Null, byte[]>>();
var consumer = provider.GetRequiredService<IConsumer<Null, byte[]>>();
var adminClient = provider.GetRequiredService<IAdminClient>();

var clients = new IClient[]
{
    producer,
    consumer,
    adminClient,
};

foreach (var client in clients)
{
    Console.WriteLine($"Resolved client: {client.Name}");
}
