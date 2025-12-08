namespace Confluent.Kafka.DependencyInjection
{
    using Confluent.Kafka.SyncOverAsync;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    sealed class GlobalProducer<TKey, TValue> : IProducer<TKey, TValue>
    {
        static readonly Type[] BuiltInTypes =
        {
            typeof(Null),
            typeof(int),
            typeof(long),
            typeof(string),
            typeof(float),
            typeof(double),
            typeof(byte[]),
        };

        readonly IEnumerable<IClientConfigProvider> configProviders;
        readonly IEnumerable<IClientBuilderSetup> builderSetups;
        readonly ISerializer<TKey> keySerializer;
        readonly ISerializer<TValue> valueSerializer;
        readonly IAsyncSerializer<TKey> asyncKeySerializer;
        readonly IAsyncSerializer<TValue> asyncValueSerializer;
        readonly object syncObj = new object();

        IProducer<TKey, TValue> producer;
        IProducer<TKey, TValue> syncProducer;

        public GlobalProducer(
            IEnumerable<IClientConfigProvider> configProviders,
            IEnumerable<IClientBuilderSetup> builderSetups,
            ISerializer<TKey> keySerializer = null,
            ISerializer<TValue> valueSerializer = null,
            IAsyncSerializer<TKey> asyncKeySerializer = null,
            IAsyncSerializer<TValue> asyncValueSerializer = null)
        {
            this.configProviders = configProviders;
            this.builderSetups = builderSetups;
            this.keySerializer = keySerializer;
            this.valueSerializer = valueSerializer;
            this.asyncKeySerializer = asyncKeySerializer;
            this.asyncValueSerializer = asyncValueSerializer;
        }

        public Handle Handle => Producer.Handle;

        public string Name => Producer.Name;

        IProducer<TKey, TValue> Producer
        {
            get
            {
                if (producer == null)
                {
                    lock (syncObj)
                    {
                        if (producer == null)
                        {
                            var config = new Dictionary<string, string>();

                            foreach (var provider in configProviders)
                            {
                                var iterator = provider.ForProducer<TKey, TValue>();

                                while (iterator.MoveNext())
                                {
                                    config[iterator.Current.Key] = iterator.Current.Value;
                                }
                            }

                            var builder = new DIBuilder(config);

                            foreach (var setup in builderSetups)
                            {
                                setup.Apply(builder);
                            }

                            if (builder.KeySerializer == null && builder.AsyncKeySerializer == null &&
                                !BuiltInTypes.Contains(typeof(TKey)))
                            {
                                if (keySerializer != null)
                                {
                                    builder.SetKeySerializer(keySerializer);
                                }
                                else if (asyncKeySerializer != null)
                                {
                                    builder.SetKeySerializer(asyncKeySerializer);
                                }
                            }

                            if (builder.ValueSerializer == null && builder.AsyncValueSerializer == null &&
                                !BuiltInTypes.Contains(typeof(TValue)))
                            {
                                if (valueSerializer != null)
                                {
                                    builder.SetValueSerializer(valueSerializer);
                                }
                                else if (asyncValueSerializer != null)
                                {
                                    builder.SetValueSerializer(asyncValueSerializer);
                                }
                            }

                            producer = builder.Build();

                            if (builder.AsyncKeySerializer != null || builder.AsyncValueSerializer != null)
                            {
                                // Workaround to support both modes of delivery handling.
                                // https://github.com/confluentinc/confluent-kafka-dotnet/issues/2481
                                var syncBuilder = new DependentProducerBuilder<TKey, TValue>(Producer.Handle);

                                if (builder.AsyncKeySerializer != null)
                                {
                                    syncBuilder.SetKeySerializer(builder.AsyncKeySerializer.AsSyncOverAsync());
                                }
                                else if (builder.KeySerializer != null)
                                {
                                    syncBuilder.SetKeySerializer(builder.KeySerializer);
                                }

                                if (builder.AsyncValueSerializer != null)
                                {
                                    syncBuilder.SetValueSerializer(builder.AsyncValueSerializer.AsSyncOverAsync());
                                }
                                else if (builder.ValueSerializer != null)
                                {
                                    syncBuilder.SetValueSerializer(builder.ValueSerializer);
                                }

                                syncProducer = syncBuilder.Build();
                            }
                        }
                    }
                }

                return producer;
            }
        }

        IProducer<TKey, TValue> SyncProducer
        {
            get
            {
                var asyncProducer = Producer;
                return syncProducer ?? asyncProducer;
            }
        }

        public int AddBrokers(string brokers)
        {
            return Producer.AddBrokers(brokers);
        }

        public void SetSaslCredentials(string username, string password)
        {
            Producer.SetSaslCredentials(username, password);
        }

        public void Produce(
            string topic,
            Message<TKey, TValue> message,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
        {
            SyncProducer.Produce(topic, message, deliveryHandler);
        }

        public void Produce(
            TopicPartition topicPartition,
            Message<TKey, TValue> message,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
        {
            SyncProducer.Produce(topicPartition, message, deliveryHandler);
        }

        public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
            string topic,
            Message<TKey, TValue> message,
            CancellationToken cancellationToken = default)
        {
            return Producer.ProduceAsync(topic, message, cancellationToken);
        }

        public Task<DeliveryResult<TKey, TValue>> ProduceAsync(
            TopicPartition topicPartition,
            Message<TKey, TValue> message,
            CancellationToken cancellationToken = default)
        {
            return Producer.ProduceAsync(topicPartition, message, cancellationToken);
        }

        public void BeginTransaction()
        {
            Producer.BeginTransaction();
        }

        public void CommitTransaction()
        {
            Producer.CommitTransaction();
        }

        public void CommitTransaction(TimeSpan timeout)
        {
            Producer.CommitTransaction(timeout);
        }

        public void AbortTransaction()
        {
            Producer.AbortTransaction();
        }

        public void AbortTransaction(TimeSpan timeout)
        {
            Producer.AbortTransaction(timeout);
        }

        public void InitTransactions(TimeSpan timeout)
        {
            Producer.InitTransactions(timeout);
        }

        public void SendOffsetsToTransaction(
            IEnumerable<TopicPartitionOffset> offsets,
            IConsumerGroupMetadata groupMetadata,
            TimeSpan timeout)
        {
            Producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
        }

        public int Poll(TimeSpan timeout)
        {
            return Producer.Poll(timeout);
        }

        public void Flush(CancellationToken cancellationToken = default)
        {
            Producer.Flush(cancellationToken);
        }

        public int Flush(TimeSpan timeout)
        {
            return Producer.Flush(timeout);
        }

        public void Dispose()
        {
            syncProducer?.Dispose();
            producer?.Dispose();
        }

        sealed class DIBuilder : ProducerBuilder<TKey, TValue>
        {
            public DIBuilder(IEnumerable<KeyValuePair<string, string>> config) : base(config)
            {
            }

            public new ISerializer<TKey> KeySerializer => base.KeySerializer;

            public new ISerializer<TValue> ValueSerializer => base.ValueSerializer;

            public new IAsyncSerializer<TKey> AsyncKeySerializer => base.AsyncKeySerializer;

            public new IAsyncSerializer<TValue> AsyncValueSerializer => base.AsyncValueSerializer;
        }
    }
}
