using Confluent.Kafka;
using Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

await using var provider = new ServiceCollection()
    .AddLogging(x => x.AddConsole())
    .AddKafkaClient(
        new Dictionary<string, string>
        {
            { "bootstrap.servers", "localhost:9092" },
            { "enable.idempotence", "true" },
            { "group.id", "group1" },
        })
    .BuildServiceProvider();

var producer = provider.GetRequiredService<IProducer<Null, byte[]>>();
var consumer = provider.GetRequiredService<IConsumer<Null, byte[]>>();

using var producer2 = provider.GetRequiredService<ProducerBuilder<Null, byte[]>>().Build();
using var consumer2 = provider.GetRequiredService<ConsumerBuilder<Null, byte[]>>().Build();

using var producer3 = provider.GetRequiredService<IKafkaFactory>()
    .CreateProducer<Null, byte[]>();

using var consumer3 = provider.GetRequiredService<IKafkaFactory>()
    .CreateConsumer<Null, byte[]>();

foreach (var client in new IClient[] { producer, producer2, producer3, consumer, consumer2, consumer3 })
{
    Console.WriteLine($"Resolved client: {client.Name}");
}

consumer2.Close();
consumer3.Close();
