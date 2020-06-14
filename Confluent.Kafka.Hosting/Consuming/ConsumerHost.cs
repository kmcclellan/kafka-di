using System.Threading;
using System.Threading.Tasks;

namespace Confluent.Kafka.Hosting.Consuming
{
    class ConsumerHost<TKey, TValue> : IHostedWorker
    {
        private readonly IConsumer<TKey, TValue> client;
        private readonly IHostedConsumer<TKey, TValue> consumer;

        public ConsumerHost(IConsumer<TKey, TValue> client, IHostedConsumer<TKey, TValue> consumer)
        {
            this.client = client;
            this.consumer = consumer;
        }

        public async Task Work(CancellationToken cancellationToken)
        {
            var message = await Read(cancellationToken);
            await consumer.ConsumeAsync(message, cancellationToken);
        }

        private async Task<Message<TKey, TValue>> Read(CancellationToken cancellationToken)
        {
            while (true)
            {
                // First see if there is an available message before spinning up a thread.
                var result = client.Consume(0) ??
                    await Task.Run(() => client.Consume(cancellationToken));

                // TODO: Support EOF handling?
                if (result.IsPartitionEOF) continue;
                return result.Message;
            }
        }
    }
}
