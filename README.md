# Kafka Hosting
An extension of [Confluent's Kafka client](https://github.com/confluentinc/confluent-kafka-dotnet) for use with `Microsoft.Extensions.Hosting` (and friends).

### Features
* Configure a Kafka consumer as an injected service.
* Inject services into Kafka event handlers and serializers.
* Configure a host to invoke/manage consumer.

## Installation

Choose a NuGet package to add to your project.

To configure a hosted consumer:

    $ dotnet add package Confluent.Kafka.Hosting

To inject a consumer as a service:

    $ dotnet add package Confluent.Kafka.DependencyInjection

## Usage

Configure a hosted consumer:
```c#
var hostBuilder = new HostBuilder();
hostBuilder.UseConsumer<MyConsumer, string, string>((_, consumer) =>
    consumer.AddConfiguration(new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "test-group"
    }).AddTopics(new[] { "test-topic" }));
```

Implement `IHostedConsumer` to process messages:
```c#
class MyConsumer : IHostedConsumer<string, string>
{
    public Task ConsumeAsync(Message<string, string> message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Consumed message: {new { message.Key, message.Value }}");
        return Task.CompletedTask;
    }
}
```

Alternatively, register a consumer as a service:
```c#
var services = new ServiceCollection();
services.AddSingleton<MyService>()
    .AddConsumer<string, string>()
        .AddConfiguration(new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group"
        }).AddTopics(new[] { "test-topic" });
```

Inject consumer via constructor:
```c#
class MyService
{
    private readonly IConsumer<string, string> consumer;
    
    public MyService(IConsumer<string, string> consumer)
    {
        this.consumer = consumer;
    }
}
```
