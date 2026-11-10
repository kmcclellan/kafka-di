namespace Confluent.Kafka
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Tracing and metrics following the OpenTelemetry
    /// <see href="https://opentelemetry.io/docs/specs/semconv/messaging/kafka/">semantic conventions for Kafka</see>.
    /// </summary>
    static class KafkaTelemetry
    {
        public const string InstrumentName = "Confluent.Kafka.DependencyInjection";

        static readonly string? Version = GetVersion();

        static readonly ActivitySource Source = new(InstrumentName, Version);
        static readonly Meter Meter = new(InstrumentName, Version);

        static readonly Counter<long> SentMessages = Meter.CreateCounter<long>(
            "messaging.client.sent.messages",
            unit: "{message}",
            description: "Number of messages producer attempted to send to the broker.");

        static readonly Counter<long> ConsumedMessages = Meter.CreateCounter<long>(
            "messaging.client.consumed.messages",
            unit: "{message}",
            description: "Number of messages that were delivered to the application.");

        static readonly Histogram<double> ProcessDuration = Meter.CreateHistogram<double>(
            "messaging.process.duration",
            unit: "s",
            description: "Duration of processing operation.",
            advice: new()
            {
                HistogramBucketBoundaries = new[]
                {
                    0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10,
                },
            });

        static readonly DistributedContextPropagator.PropagatorSetterCallback InjectHeader =
            (carrier, key, value) => ((Headers)carrier!).Add(key, Encoding.UTF8.GetBytes(value));

        static readonly DistributedContextPropagator.PropagatorGetterCallback ExtractHeader =
            (object? carrier, string fieldName, out string? fieldValue, out IEnumerable<string>? fieldValues) =>
            {
                var values = default(List<string>);

                foreach (var header in (Headers)carrier!)
                {
                    if (header.Key != fieldName)
                    {
                        continue;
                    }

                    var bytes = header.GetValueBytes();

                    if (bytes == null)
                    {
                        continue;
                    }

                    values ??= new();
                    values.Add(Encoding.UTF8.GetString(bytes));
                }

                if (values?.Count == 1)
                {
                    fieldValue = values[0];
                    fieldValues = null;
                }
                else
                {
                    fieldValue = null;
                    fieldValues = values;
                }
            };

        public static bool HasProduceListeners => Source.HasListeners() || SentMessages.Enabled;

        public static Activity? BeginProduce<TKey, TValue>(
            string clientId,
            string topic,
            ref Message<TKey, TValue> message)
        {
            if (!Source.HasListeners())
            {
                return null;
            }

            var activity = Source.StartActivity(
                $"produce {topic}",
                ActivityKind.Producer,
                parentContext: Activity.Current?.Context ?? default,
                GetTags("produce", "send", topic, clientId: clientId));

            if (Activity.Current != null)
            {
                var propagator = DistributedContextPropagator.Current;
                var headers = new Headers();

                if (message.Headers != null)
                {
                    foreach (var header in message.Headers)
                    {
                        // Ignore context from a re-sent message.
                        if (!propagator.Fields.Contains(header.Key))
                        {
                            headers.Add(header.Key, header.GetValueBytes());
                        }
                    }
                }

                propagator.Inject(Activity.Current, headers, InjectHeader);

                // Clone the message so we don't modify caller state.
                message = new()
                {
                    Key = message.Key,
                    Value = message.Value,
                    Timestamp = message.Timestamp,
                    Headers = headers,
                };
            }

            return activity;
        }

        public static void EndProduce<TKey, TValue>(Activity? activity, DeliveryResult<TKey, TValue> result)
        {
            if (activity != null)
            {
                SetOffset(activity, result.Partition, result.Offset);
            }

            if (SentMessages.Enabled)
            {
                SentMessages.Add(delta: 1, GetTags("produce", topic: result.Topic, partition: result.Partition));
            }
        }

        public static void EndProduce<TKey, TValue>(Activity? activity, DeliveryReport<TKey, TValue> report)
        {
            if (activity != null)
            {
                SetOffset(activity, report.Partition, report.Offset);

                if (report.Error != null && report.Error.IsError)
                {
                    SetError(activity, report.Error);
                }
            }

            if (SentMessages.Enabled)
            {
                var errorType = report.Error != null && report.Error.IsError ? report.Error.Code.ToString() : null;
                var tags = GetTags("produce", topic: report.Topic, partition: report.Partition, errorType: errorType);
                SentMessages.Add(delta: 1, tags);
            }
        }

        public static void EndProduce<TKey, TValue>(Activity? activity, Exception exception)
        {
            var result = exception is ProduceException<TKey, TValue> produceException
                ? produceException.DeliveryResult
                : null;

            var kafkaError = exception is KafkaException kafkaException ? kafkaException.Error : null;

            if (activity != null)
            {
                if (result != null)
                {
                    SetOffset(activity, result.Partition, result.Offset);
                }

                if (kafkaError != null)
                {
                    SetError(activity, kafkaError, exception);
                }
                else
                {
                    SetError(activity, exception);
                }
            }

            if (kafkaError != null && result != null && SentMessages.Enabled)
            {
                var errorType = kafkaError.Code.ToString();
                var tags = GetTags("produce", topic: result.Topic, partition: result.Partition, errorType: errorType);
                SentMessages.Add(delta: 1, tags);
            }
        }

        public static void Consume<TKey, TValue>(ConsumeResult<TKey, TValue> result)
        {
            if (!result.IsPartitionEOF && ConsumedMessages.Enabled)
            {
                var tags = GetTags("consume", topic: result.Topic, partition: result.Partition);
                ConsumedMessages.Add(delta: 1, tags);
            }
        }

        public static void Consume(ConsumeException exception)
        {
            if (exception.ConsumerRecord != null && exception.Error != null && ConsumedMessages.Enabled)
            {
                var tags = GetTags(
                    "consume",
                    topic: exception.ConsumerRecord.Topic,
                    partition: exception.ConsumerRecord.Partition,
                    errorType: exception.Error.Code.ToString());

                ConsumedMessages.Add(delta: 1, tags);
            }
        }

        public static Activity? BeginProcess<TKey, TValue>(
            string clientId,
            ConsumeResult<TKey, TValue> result,
            bool linkProduce)
        {
            if (result.IsPartitionEOF || !Source.HasListeners())
            {
                return null;
            }

            var parentContext = Activity.Current?.Context ?? default;
            var links = default(IEnumerable<ActivityLink>);
            var baggage = default(IEnumerable<KeyValuePair<string, string?>>);

            if (result.Message?.Headers != null)
            {
                var propagator = DistributedContextPropagator.Current;

                propagator.ExtractTraceIdAndState(
                    result.Message.Headers,
                    ExtractHeader,
                    out var traceParent,
                    out var traceState);

                if (ActivityContext.TryParse(traceParent, traceState, isRemote: true, out var remoteContext))
                {
                    if (linkProduce)
                    {
                        links = new[] { new ActivityLink(remoteContext) };
                    }
                    else
                    {
                        parentContext = remoteContext;
                    }
                }

                baggage = propagator.ExtractBaggage(result.Message.Headers, ExtractHeader);
            }

            var tags = GetTags(
                "process",
                "process",
                result.Topic,
                result.Partition,
                result.Offset,
                clientId);

            var activity = Source.StartActivity(
                $"process {result.Topic}",
                ActivityKind.Consumer,
                parentContext,
                tags,
                links);

            if (activity != null && baggage != null)
            {
                foreach (var pair in baggage)
                {
                    activity.AddBaggage(pair.Key, pair.Value);
                }
            }

            return activity;
        }

        public static void EndProcess<TKey, TValue>(
            ConsumeResult<TKey, TValue> result,
            Stopwatch watch,
            Activity? activity,
            Exception? exception = null)
        {
            if (result.IsPartitionEOF || exception is OperationCanceledException)
            {
                return;
            }

            if (activity != null && exception != null)
            {
                SetError(activity, exception);
            }

            if (ProcessDuration.Enabled)
            {
                var tags = GetTags(
                    "process",
                    topic: result.Topic,
                    partition: result.Partition,
                    errorType: exception?.GetType().ToString());

                ProcessDuration.Record(watch.Elapsed.TotalSeconds, tags);
            }
        }

        static TagList GetTags(
            string operationName,
            string? operationType = null,
            string? topic = null,
            Partition? partition = null,
            Offset? offset = null,
            string? clientId = null,
            string? errorType = null)
        {
            var tags = new TagList
            {
                { "messaging.system.name", "kafka" },
                { "messaging.operation.name", operationName },
            };

            if (operationType != null)
            {
                tags.Add("messaging.operation.type", operationType);
            }

            if (topic != null)
            {
                tags.Add("messaging.destination.name", topic);
            }

            if (partition != null && !partition.Value.IsSpecial)
            {
                tags.Add(
                    "messaging.destination.partition.id",
                    partition.Value.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (offset != null && !offset.Value.IsSpecial)
            {
                tags.Add("kafka.offset", offset.Value.Value);
            }

            if (clientId != null)
            {
                tags.Add("messaging.client.id", clientId);
            }

            if (errorType != null)
            {
                tags.Add("error.type", errorType);
            }

            return tags;
        }

        static void SetOffset(Activity activity, Partition partition, Offset offset)
        {
            if (!partition.IsSpecial)
            {
                activity.SetTag(
                    "messaging.destination.partition.id",
                    partition.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (!offset.IsSpecial)
            {
                activity.SetTag("kafka.offset", offset.Value);
            }
        }

        static void SetError(Activity activity, Error error, Exception? exception = null)
        {
            if (error.IsError || exception != null)
            {
                activity.SetTag("error.type", error.Code.ToString());
                activity.SetStatus(ActivityStatusCode.Error, error.ToString());

                if (exception != null)
                {
                    activity.AddException(exception);
                }
            }
        }

        static void SetError(Activity activity, Exception exception)
        {
            activity.SetTag("error.type", exception.GetType().ToString());
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.AddException(exception);
        }

        static string? GetVersion()
        {
            var infoAttribute = typeof(KafkaTelemetry).GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (infoAttribute != null)
            {
                // Strip source revision metadata (e.g. "1.2.3+abcdef").
                return infoAttribute.InformationalVersion.Split('+')[0];
            }

            return null;
        }
    }
}
