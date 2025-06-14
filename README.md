# Kafka Dependency Injection
An extension of [Confluent's Kafka client](https://github.com/confluentinc/confluent-kafka-dotnet) for use with `Microsoft.Extensions.DependencyInjection` (and friends).

### Features
* Configure/resolve Kafka clients using the service container.
* Load client config properties using `Microsoft.Extensions.Configuration`.
* Automatically log client events using `Microsoft.Extensions.Logging`.

## Installation

Add the NuGet package to your project:

    $ dotnet add package Confluent.Kafka.DependencyInjection

## Usage

### Resolving clients

Kafka DI works out-of-the-box after registering services with an `IServiceCollection`.

```c#
services.AddKafkaClient();
services.AddTransient<MyService>();
```

Inject Kafka clients via constructor.

```c#
public MyService(IProducer<string, byte[]> producer, IConsumer<Ignore, MyType> consumer, IAdminClient adminClient)
{
    // Clients are singletons managed by the container.
    Producer = producer;
    Consumer = consumer;
    AdminClient = adminClient;
}
```

### Configuring clients

Client configuration properties are bound to the `Kafka` section of .NET configuration providers, such as `appsettings.json`.

```json
{
  "Kafka": {
    "Producer": {
      "bootstrap.servers": "localhost:9092",
      "transactional.id": "example"
    },
    "Consumer": {
      "bootstrap.servers": "localhost:9092",
      "group.id": "example"
    },
    "Admin": {
      "bootstrap.servers": "localhost:9092"
    }
  }
}
```

You can also specify configuration properties using the options pattern.

```c#
// Prepare consumers for manual offset storage.
services.Configure<ConsumerConfig>(x => x.EnableAutoOffsetStore = false);
```

Configure serialization by registering the appropriate interface.

```c#
// "Open" generic registrations apply to all key/value types (except built-in types).
services.AddTransient(typeof(IAsyncDeserializer<>), typeof(JsonDeserializer<>));

// Configure schema registry (required by Confluent serializers).
services.AddSingleton<ISchemaRegistryClient>(
    x => new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = "localhost:8081" }));
```

For advanced scenarios, implement `IClientBuilderSetup` to customize clients further.

```c#
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
```

Register custom setup with services.

```c#
services.AddTransient<IClientBuilderSetup, MyClientSetup>();
```
