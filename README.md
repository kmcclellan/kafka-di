# Kafka Dependency Injection
An extension of [Confluent's Kafka client](https://github.com/confluentinc/confluent-kafka-dotnet) for use with `Microsoft.Extensions.DependencyInjection` (and friends).

### Features
* Configure Kafka producers/consumers using `Microsoft.Extensions.DependencyInjection.IServiceCollection`.
* Default logging of asynchronous Kafka events through `Microsoft.Extensions.Logging.ILogger`.

## Installation

Add the NuGet package to your project:

    $ dotnet add package Confluent.Kafka.DependencyInjection

## Usage

Add a global Kafka client:

```c#
services.AddKafkaClient(new Dictionary<string, string>
{
    { "bootstrap.servers", "localhost:9092" },
    { "enable.idempotence", "true" },
    { "group.id", "group1" }
});
```

Alternatively, add typed clients with distinct configurations:

```c#
services.AddKafkaClient<MyService>(new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    EnableIdempotence = true
});

services.AddKafkaClient<MyOtherService>(new ConsumerConfig
{
    BootstrapServers = "somewhere.else:9092",
    GroupId = "group1"
});
```

Optionally, configure message serialization:

```c#
// Use open generics to apply to all keys and values.
services.AddSingleton(typeof(IAsyncDeserializer<>), typeof(AvroDeserializer<>));

// Use closed generics to select type-specific serializers.
services.AddSingleton<IAsyncSerializer<MyType>, JsonSerializer<MyType>>();

// Synchronous serializers take precedence, if present.
services.AddSingleton(sp => sp.GetRequiredService<IAsyncSerializer<MyType>>().AsSyncOverAsync());

// Configure schema registry (required by some serializers).
services.AddSingleton<ISchemaRegistryClient>(sp =>
    new CachedSchemaRegistryClient(new SchemaRegistryConfig
    {
        Url = "localhost:8081"
    }));
```

Optionally, configure custom handlers for Kafka events:

```c#
services.AddTransient<IErrorHandler, MyHandler>()
    .AddTransient<IStatisticsHandler, MyHandler>()
    .AddTransient<ILogHandler, MyHandler>()
    .AddTransient<IPartitionsAssignedHandler, MyHandler>()
    .AddTransient<IPartitionsRevokedHandler, MyHandler>()
    .AddTransient<IOffsetsCommittedHandler, MyHandler>();
```

Inject producers/consumers via constructor:

```c#
public MyService(IProducer<Null, string> producer)
{
    // Producer is a singleton managed by the container.
    this.producer = producer;
}
```

Alternatively, inject `IKafkaFactory` to override configuration and control lifespan:

```c#
using var consumer = factory.CreateConsumer<MyType, MyOtherType>(new ConsumerConfig
{
    GroupId = "group2"
});

// ...
// Remember to close manually created consumers.
consumer.Close();
```
