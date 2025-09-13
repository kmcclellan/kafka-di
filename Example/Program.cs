using Confluent.Kafka;
using Confluent.Kafka.DependencyInjection;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<IConfiguration>(
    x => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

builder.Services.AddKafkaClient();
builder.Services.AddTransient<MyService>();

// Prepare consumers for manual offset storage.
builder.Services.Configure<ConsumerConfig>(x => x.EnableAutoOffsetStore = false);

// "Open" generic registrations apply to all key/value types (except built-in types).
builder.Services.AddTransient(typeof(IAsyncDeserializer<>), typeof(JsonDeserializer<>));

// Configure schema registry (required by Confluent serializers).
builder.Services.AddSingleton<ISchemaRegistryClient>(
    x => new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = "localhost:8081" }));

builder.Services.AddTransient<IClientBuilderSetup, MyClientSetup>();
builder.Services.AddHostedService<MyWorker>();

IHost host;

await using ((IAsyncDisposable)(host = builder.Build()))
{
    var test = host.Services.GetRequiredService<MyService>();

    foreach (var client in new IClient[] { test.Producer, test.Consumer, test.AdminClient })
    {
        Console.WriteLine("Resolved client '{0}'", client.Name);
    }

    await host.RunAsync();
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

class MyWorker(IConsumer<Ignore, MyType> consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Consumer service started.");

        // ConsumeAllAsync() is an extension provided by this library.
        await foreach (var result in consumer.ConsumeAllAsync().WithCancellation(stoppingToken))
        {
            Console.WriteLine($"Message {result.TopicPartitionOffset} processed.");
        }
    }
}
