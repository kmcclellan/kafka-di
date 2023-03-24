# Kafka Dependency Injection
An extension of [Confluent's Kafka client](https://github.com/confluentinc/confluent-kafka-dotnet) for use with `Microsoft.Extensions.DependencyInjection` (and friends).

### Features
* Inject/resolve Kafka clients using the service container.
* Configure Kafka clients using the options pattern.
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
services.AddSingleton<MyService>();
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

Client config properties are bound to the `Kafka` section of .NET configuration providers, such as `appsettings.json`.

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

You can also leverage `KafkaClientOptions` to customize clients further, including serialization and event handlers.

```c#
var builder = services.AddKafkaClient()

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
```
