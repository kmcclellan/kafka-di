# Kafka Dependency Injection
An extension of [Confluent's Kafka client](https://github.com/confluentinc/confluent-kafka-dotnet) for use with `Microsoft.Extensions.DependencyInjection` (and friends).

### Features
* Configure/resolve Kafka clients using the service container.
* Load client config properties using `Microsoft.Extensions.Configuration`.
* Automatically log client events using `Microsoft.Extensions.Logging`.
* Extend a base hosted service to consume/process Kafka messages with `Microsoft.Extensions.Hosting`.

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

### Consumer hosting

Once the client is configured, implement `ConsumerService` to integrate with the .NET host.

```c#
class MyWorker(
    IConsumer<Ignore, MyType> consumer,
    IOptions<ConsumerHostingOptions> options,
    ILogger<MyWorker> logger) :
    ConsumerService<Ignore, MyType>(consumer, options, logger)
{
    protected override ValueTask ProcessAsync(
        ConsumeResult<Ignore, MyType> result,
        CancellationToken cancellationToken)
    {
        // Process the message.
        return ValueTask.CompletedTask;
    }
}
```

Register the service with the container.

```c#
builder.Services.AddHostedService<MyWorker>();

// Bind consumer hosting configuration.
var hostingOptions = builder.Services.AddOptions<ConsumerHostingOptions>()
    .BindConfiguration("Kafka:Hosting");
```

Hosted consumer features:

* Configurable parallelism while preserving order for each Kafka message key.
* Parallel-safe offset storage to achieve ["at least once" delivery semantics](https://docs.confluent.io/kafka/design/delivery-semantics.html#consumer-receipt).

```json
{
  "Kafka": {
    "Consumer": {
      "bootstrap.servers": "localhost:9092",
      "group.id": "example",
      "enable.auto.offset.store": "false"
    },
    "Hosting": {
      "Disabled": false,
      "Subscription": [ "my-topic" ],
      "MaxDegreeOfParallelism": 10,
      "StoreProcessedOffsets": true
    }
  }
}

```
