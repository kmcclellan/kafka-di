namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics about a Kafka client and its connected cluster.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#general-structure">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class KafkaStatistics
{
    /// <summary>
    /// Handle instance name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The configured (or default) client.id.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    /// <summary>
    /// Instance type (producer or consumer).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// librdkafka's internal monotonic clock (microseconds).
    /// </summary>
    [JsonPropertyName("ts")]
    public long Ticks { get; set; }

    /// <summary>
    /// Wall clock time in seconds since the epoch.
    /// </summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    /// <summary>
    /// Time since this client instance was created (microseconds).
    /// </summary>
    [JsonPropertyName("age")]
    public long Age { get; set; }

    /// <summary>
    /// Number of ops (callbacks, events, etc) waiting in queue for application to serve with rd_kafka_poll().
    /// </summary>
    [JsonPropertyName("replyq")]
    public long ReplyQueue { get; set; }

    /// <summary>
    /// Current number of messages in producer queues.
    /// </summary>
    [JsonPropertyName("msg_cnt")]
    public long MessageCount { get; set; }

    /// <summary>
    /// Current total size of messages in producer queues.
    /// </summary>
    [JsonPropertyName("msg_size")]
    public long MessageSize { get; set; }

    /// <summary>
    /// Threshold: maximum number of messages allowed on the producer queues.
    /// </summary>
    [JsonPropertyName("msg_max")]
    public long MessageMax { get; set; }

    /// <summary>
    /// Threshold: maximum total size of messages allowed on the producer queues.
    /// </summary>
    [JsonPropertyName("msg_size_max")]
    public long MessageSizeMax { get; set; }

    /// <summary>
    /// Total number of requests sent to Kafka brokers.
    /// </summary>
    [JsonPropertyName("tx")]
    public long TransmittedRequests { get; set; }

    /// <summary>
    /// Total number of bytes transmitted to Kafka brokers.
    /// </summary>
    [JsonPropertyName("tx_bytes")]
    public long TransmittedBytes { get; set; }

    /// <summary>
    /// Total number of responses received from Kafka brokers.
    /// </summary>
    [JsonPropertyName("rx")]
    public long ReceivedResponses { get; set; }

    /// <summary>
    /// Total number of bytes received from Kafka brokers.
    /// </summary>
    [JsonPropertyName("rx_bytes")]
    public long ReceivedBytes { get; set; }

    /// <summary>
    /// Total number of messages transmitted (produced) to Kafka brokers.
    /// </summary>
    [JsonPropertyName("txmsgs")]
    public long TransmittedMessages { get; set; }

    /// <summary>
    /// Total number of message bytes (including framing, such as per-Message framing and MessageSet/batch framing) transmitted to Kafka brokers.
    /// </summary>
    [JsonPropertyName("txmsg_bytes")]
    public long TransmittedMessageBytes { get; set; }

    /// <summary>
    /// Total number of messages consumed, not including ignored messages (due to offset, etc), from Kafka brokers.
    /// </summary>
    [JsonPropertyName("rxmsgs")]
    public long ReceivedMessages { get; set; }

    /// <summary>
    /// Total number of message bytes (including framing) received from Kafka brokers.
    /// </summary>
    [JsonPropertyName("rxmsg_bytes")]
    public long ReceivedMessageBytes { get; set; }

    /// <summary>
    /// Internal tracking of legacy vs new consumer API state.
    /// </summary>
    [JsonPropertyName("simple_cnt")]
    public long SimpleCount { get; set; }

    /// <summary>
    /// Number of topics in the metadata cache.
    /// </summary>
    [JsonPropertyName("metadata_cache_cnt")]
    public long MetadataCacheCount { get; set; }

    /// <summary>
    /// Dict of brokers, key is broker name, value is object.
    /// </summary>
    [JsonPropertyName("brokers")]
    public IReadOnlyDictionary<string, BrokerStatistics>? Brokers { get; set; }

    /// <summary>
    /// Dict of topics, key is topic name, value is object.
    /// </summary>
    [JsonPropertyName("topics")]
    public IReadOnlyDictionary<string, TopicStatistics>? Topics { get; set; }

    /// <summary>
    /// Consumer group metrics.
    /// </summary>
    [JsonPropertyName("cgrp")]
    public ConsumerGroupStatistics? ConsumerGroup { get; set; }

    /// <summary>
    /// EOS / Idempotent producer state and metrics.
    /// </summary>
    [JsonPropertyName("eos")]
    public ExactlyOnceSemanticsStatistics? ExactlyOnceSemantics { get; set; }
}
