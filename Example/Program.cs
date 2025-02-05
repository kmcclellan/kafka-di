using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(
    x => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

var builder = services.AddKafkaClient();

builder.Configure(
    options =>
    {
        // Config properties apply to all clients with a matching type (consumers, in this case).
        options.Configure(new ConsumerConfig { StatisticsIntervalMs = 5000 });

        // Optionally, configure handlers for asynchronous client events.
        options.OnStatistics((x, y) => Console.WriteLine(y));
    });

// Optionally, configure serialization for specific types.
builder.Configure<JsonDeserializer<MyType>>((x, y) => x.Deserialize(y));
services.AddSingleton(typeof(JsonDeserializer<>));

// Configure schema registry (required by some serializers).
services.AddSingleton<ISchemaRegistryClient>(
    x => new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = "localhost:8081" }));

services.AddSingleton<MyService>();

await using var provider = services.BuildServiceProvider();
var test = provider.GetRequiredService<MyService>();

foreach (var client in new IClient[] { test.Producer, test.Consumer, test.AdminClient })
{
    Console.WriteLine("Resolved client '{0}'", client.Name);
}

class MyService(IProducer<string, byte[]> producer, IConsumer<Ignore, MyType> consumer, IAdminClient adminClient)
{
    public IProducer<string, byte[]> Producer { get; } = producer;

    public IConsumer<Ignore, MyType> Consumer { get; } = consumer;

    public IAdminClient AdminClient { get; } = adminClient;
}

class MyType
{
    public string Data { get; set; }
}
