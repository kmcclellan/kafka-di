using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Consuming
{
    interface IConsumerProvider
    {
        public IEnumerable<KeyValuePair<string, string>>? Configuration { set; }
        public IEnumerable<string>? Topics { set; }
        public bool EnableLogging { set; }
    }

    class ConsumerProvider<TKey, TValue> : IConsumerProvider
    {
        public IEnumerable<KeyValuePair<string, string>>? Configuration { private get; set; }
        public IEnumerable<string>? Topics { private get; set; }
        public bool EnableLogging { private get; set; }

        public IConsumer<TKey, TValue> Build(IServiceProvider services)
        {
            var builder = new ConsumerBuilder<TKey, TValue>(Configuration);

            services.TryService<IDeserializer<TKey>>(d => builder.SetKeyDeserializer(d))
                .TryService<IDeserializer<TValue>>(d => builder.SetValueDeserializer(d))
                .TryService<IStatisticsHandler<IConsumer<TKey, TValue>>>(h => builder.SetStatisticsHandler(h.OnStatistics))
                .TryService<IErrorHandler<IConsumer<TKey, TValue>>>(h => builder.SetErrorHandler(h.OnError))
                .TryService<IOffsetsCommittedHandler<TKey, TValue>>(h => builder.SetOffsetsCommittedHandler(h.OnOffsetsCommitted))
                .TryService<IPartitionsAssignedHandler<TKey, TValue>>(h => builder.SetPartitionsAssignedHandler(h.OnPartitionsAssigned))
                .TryService<IPartitionsRevokedHandler<TKey, TValue>>(h => builder.SetPartitionsRevokedHandler(h.OnPartitionsRevoked));

            if (EnableLogging)
            {
                var logger = services.GetRequiredService<ILogger<IConsumer<TKey, TValue>>>();
                builder.SetLogHandler((_, message) =>
                {
                    var level = (LogLevel)message.LevelAs(LogLevelType.MicrosoftExtensionsLogging);
                    logger.Log(level, "{Client}|{Facility}: " + message.Message, message.Name, message.Facility);
                });
            }

            var consumer = builder.Build();
            if (Topics != null) consumer.Subscribe(Topics);
            return consumer;
        }
    }
}
