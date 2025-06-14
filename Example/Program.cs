using Confluent.Kafka;
using Confluent.Kafka.Options;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(
    x => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

services.AddKafkaClient();
services.AddTransient<MyService>();

// Prepare consumers for manual offset storage.
services.Configure<ConsumerConfig>(x => x.EnableAutoOffsetStore = false);

// "Open" generic registrations apply to all key/value types (except built-in types).
services.AddTransient(typeof(IAsyncDeserializer<>), typeof(JsonDeserializer<>));

// Configure schema registry (required by Confluent serializers).
services.AddSingleton<ISchemaRegistryClient>(
    x => new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = "localhost:8081" }));

services.AddTransient<IClientBuilderSetup, MyClientSetup>();

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

class MyClientSetup : IClientBuilderSetup
{
    public void Apply<TKey, TValue>(ProducerBuilder<TKey, TValue> builder)
    {
        builder.SetStatisticsHandler(OnStatistics);
    }

    public void Apply<TKey, TValue>(ConsumerBuilder<TKey, TValue> builder)
    {
        builder.SetStatisticsHandler(OnStatistics);
    }

    public void Apply(AdminClientBuilder builder)
    {
        builder.SetStatisticsHandler(OnStatistics);
    }

    void OnStatistics(IClient client, string statistics)
    {
        Console.WriteLine($"New statistics available for {client.Name}");
    }
}
