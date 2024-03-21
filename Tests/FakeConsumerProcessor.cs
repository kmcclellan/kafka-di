namespace Tests;

using Confluent.Kafka;
using Confluent.Kafka.Hosting;

using System.Collections.Concurrent;

sealed class FakeConsumerProcessor<TKey, TValue>
{
    readonly ConcurrentQueue<ProcessSetup> setups = new();
    readonly ConcurrentQueue<ConsumeResult<TKey, TValue>> processed = new();

    public FakeConsumerProcessor()
    {
        Instance = new Implementation(this);
    }

    public IConsumerProcessor<TKey, TValue> Instance { get; }

    public void Setup(ConsumeResult<TKey, TValue> result, Task task, Action? callback = null)
    {
        setups.Enqueue(new(result, task, callback));
    }

    public void Verify(ConsumeResult<TKey, TValue> result)
    {
        if (!processed.TryDequeue(out var actual) || actual != result)
        {
            Assert.Fail($"Consume result was not processed: {result.TopicPartitionOffset}");
        }
    }

    record ProcessSetup(ConsumeResult<TKey, TValue> Result, Task Task, Action? Callback);

    sealed class Implementation(FakeConsumerProcessor<TKey, TValue> parent) : IConsumerProcessor<TKey, TValue>
    {
        public async ValueTask ProcessAsync(ConsumeResult<TKey, TValue> result, CancellationToken cancellationToken)
        {
            if (!parent.setups.TryDequeue(out var setup) || setup.Result != result)
            {
                Assert.Fail($"Unexpected consume result: {result.TopicPartitionOffset}");
            }

            setup.Callback?.Invoke();
            await setup.Task;

            parent.processed.Enqueue(result);
        }
    }
}
