using Confluent.Kafka;
using Confluent.Kafka.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await using var provider = new ServiceCollection()
    .AddLogging(x => x.AddConsole())
    .AddKafkaClient(
        new Dictionary<string, string>
        {
            { "client.id", "global" },
            { "bootstrap.servers", "localhost:9092" },
            { "enable.idempotence", "true" },
            { "group.id", "group1" },
        })
    .AddKafkaClient<TestClient1>(new ClientConfig { ClientId = "client1"  })
    .AddKafkaClient<TestClient2>(new ClientConfig { ClientId = "client2"  })
    .BuildServiceProvider();

var producer = provider.GetRequiredService<IProducer<Null, byte[]>>();
var consumer = provider.GetRequiredService<IConsumer<Null, byte[]>>();
var adminClient = provider.GetRequiredService<IAdminClient>();

using var producer2 = provider.GetRequiredService<ProducerBuilder<Null, byte[]>>().Build();
using var consumer2 = provider.GetRequiredService<ConsumerBuilder<Null, byte[]>>().Build();

using var producer3 = provider.GetRequiredService<IKafkaFactory>()
    .CreateProducer<Null, byte[]>(new ClientConfig { ClientId = "factory1" });

using var consumer3 = provider.GetRequiredService<IKafkaFactory>()
    .CreateConsumer<Null, byte[]>(new ClientConfig { ClientId = "factory2" });

var producer4 = provider.GetRequiredService<TestClient1>().Producer;
var consumer4 = provider.GetRequiredService<TestClient2>().Consumer;

var clients = new IClient[]
{
    producer,
    producer2,
    producer3,
    producer4,
    consumer,
    consumer2,
    consumer3,
    consumer4,
    adminClient,
};

foreach (var client in clients)
{
    Console.WriteLine($"Resolved client: {client.Name}");
}

consumer2.Close();
consumer3.Close();

class TestClient1
{
    public TestClient1(IProducer<Null, Null> producer)
    {
        this.Producer = producer;
    }

    public IProducer<Null, Null> Producer { get; }
}

class TestClient2
{
    public TestClient2(IConsumer<Null, Null> consumer)
    {
        this.Consumer = consumer;
    }

    public IConsumer<Null, Null> Consumer { get; }
}
