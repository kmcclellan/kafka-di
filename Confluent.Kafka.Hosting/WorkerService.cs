using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Confluent.Kafka.Hosting
{
    class WorkerService<TWorker> : IHostedService, IDisposable where TWorker : IHostedWorker
    {
        private readonly ILogger<WorkerService<TWorker>> logger;
        private readonly IHostApplicationLifetime lifetime;
        private readonly TWorker worker;

        private readonly Task task;
        private readonly TaskCompletionSource<object?> start = new TaskCompletionSource<object?>();
        private readonly CancellationTokenSource disposal = new CancellationTokenSource();

        public WorkerService(
            ILogger<WorkerService<TWorker>> logger,
            IHostApplicationLifetime lifetime,
            TWorker worker)
        {
            this.logger = logger;
            this.lifetime = lifetime;
            this.worker = worker;
            task = Run(disposal.Token);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            start.TrySetResult(null);
            logger.LogInformation("Worker service started.");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Await twice to throw any exceptions.
            await await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
            logger.LogInformation("Worker service stopped.");
        }

        public void Dispose()
        {
            disposal.Cancel();
            disposal.Dispose();
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            await await Task.WhenAny(start.Task, Task.Delay(Timeout.Infinite, cancellationToken));

            using var workerCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, lifetime.ApplicationStopping);

            while (!lifetime.ApplicationStopping.IsCancellationRequested)
            {
                try
                {
                    await worker.Work(workerCancellation.Token);
                }
                catch (OperationCanceledException) when (workerCancellation.IsCancellationRequested)
                {
                    if (cancellationToken.IsCancellationRequested) throw;
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Unhandled exception in worker. Stopping application...");
                    lifetime.StopApplication();
                }
            }
        }
    }
}
