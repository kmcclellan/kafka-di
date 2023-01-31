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

Console.WriteLine($"Resolved clients: {producer.Name}, {consumer.Name}");
