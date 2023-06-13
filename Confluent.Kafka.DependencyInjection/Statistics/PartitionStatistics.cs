namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics about a Kafka partition.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#partitions">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class PartitionStatistics
{
    /// <summary>
    /// Partition Id (-1 for internal UA/UnAssigned partition).
    /// </summary>
    [JsonPropertyName("partition")]
    public long Partition { get; set; } = -1;

    /// <summary>
    /// The id of the broker that messages are currently being fetched from.
    /// </summary>
    [JsonPropertyName("broker")]
    public long Broker { get; set; } = -1;

    /// <summary>
    /// Current leader broker id.
    /// </summary>
    [JsonPropertyName("leader")]
    public long Leader { get; set; } = -1;

    /// <summary>
    /// Partition is explicitly desired by application.
    /// </summary>
    [JsonPropertyName("desired")]
    public bool Desired { get; set; }

    /// <summary>
    /// Partition not seen in topic metadata from broker.
    /// </summary>
    [JsonPropertyName("unknown")]
    public bool Unknown { get; set; }

    /// <summary>
    /// Number of messages waiting to be produced in first-level queue.
    /// </summary>
    [JsonPropertyName("msgq_cnt")]
    public long MessageQueueCount { get; set; }

    /// <summary>
    /// Number of bytes in the first-level queue.
    /// </summary>
    [JsonPropertyName("msgq_bytes")]
    public long MessageQueueBytes { get; set; }

    /// <summary>
    /// Number of messages ready to be produced in transmit queue.
    /// </summary>
    [JsonPropertyName("xmit_msgq_cnt")]
    public long TransmitMessageQueueCount { get; set; }

    /// <summary>
    /// Number of bytes in the transmit queue.
    /// </summary>
    [JsonPropertyName("xmit_msgq_bytes")]
    public long TransmitMessageQueueBytes { get; set; }

    /// <summary>
    /// Number of pre-fetched messages in fetch queue.
    /// </summary>
    [JsonPropertyName("fetchq_cnt")]
    public long FetchQueueCount { get; set; }

    /// <summary>
    /// Bytes in the fetch queue.
    /// </summary>
    [JsonPropertyName("fetchq_size")]
    public long FetchQueueSize { get; set; }

    /// <summary>
    /// Consumer fetch state for this partition (none, stopping, stopped, offset-query, offset-wait, active).
    /// </summary>
    [JsonPropertyName("fetch_state")]
    public string? FetchState { get; set; }

    /// <summary>
    /// Current/Last logical offset query.
    /// </summary>
    [JsonPropertyName("query_offset")]
    public long QueryOffset { get; set; } = -1;

    /// <summary>
    /// Next offset to fetch.
    /// </summary>
    [JsonPropertyName("next_offset")]
    public long NextOffset { get; set; } = -1;

    /// <summary>
    /// Offset of last message passed to application + 1.
    /// </summary>
    [JsonPropertyName("app_offset")]
    public long AppOffset { get; set; } = -1;

    /// <summary>
    /// Offset to be committed.
    /// </summary>
    [JsonPropertyName("stored_offset")]
    public long StoredOffset { get; set; } = -1;

    /// <summary>
    /// Partition leader epoch of stored offset.
    /// </summary>
    [JsonPropertyName("stored_leader_epoch")]
    public long StoredLeaderEpoch { get; set; } = -1;

    /// <summary>
    /// Last committed offset.
    /// </summary>
    [JsonPropertyName("committed_offset")]
    public long CommittedOffset { get; set; } = -1;

    /// <summary>
    /// Partition leader epoch of committed offset
    /// </summary>
    [JsonPropertyName("committed_leader_epoch")]
    public long CommittedLeaderEpoch { get; set; } = -1;

    /// <summary>
    /// Last PARTITION_EOF signaled offset.
    /// </summary>
    [JsonPropertyName("eof_offset")]
    public long EofOffset { get; set; } = -1;

    /// <summary>
    /// Partition's low watermark offset on broker.
    /// </summary>
    [JsonPropertyName("lo_offset")]
    public long LowOffset { get; set; } = -1;

    /// <summary>
    /// Partition's high watermark offset on broker.
    /// </summary>
    [JsonPropertyName("hi_offset")]
    public long HighOffset { get; set; } = -1;

    /// <summary>
    /// Partition's last stable offset on broker, or same as high offset is broker version is less than 0.11.0.0.
    /// </summary>
    [JsonPropertyName("ls_offset")]
    public long LastStableOffset { get; set; } = -1;

    /// <summary>
    /// Difference between (HighOffset or LastStableOffset) and CommittedOffset. HighOffset is used when isolation.level=read_uncommitted, otherwise LastStableOffset.
    /// </summary>
    [JsonPropertyName("consumer_lag")]
    public long ConsumerLag { get; set; }

    /// <summary>
    /// Difference between (HighOffset or LastStableOffset) and StoredOffset. See ConsumerLag and StoredOffset.
    /// </summary>
    [JsonPropertyName("consumer_lag_stored")]
    public long ConsumerLagStored { get; set; }

    /// <summary>
    /// Last known partition leader epoch, or -1 if unknown.
    /// </summary>
    [JsonPropertyName("leader_epoch")]
    public long LeaderEpoch { get; set; } = -1;

    /// <summary>
    /// Total number of messages transmitted (produced).
    /// </summary>
    [JsonPropertyName("txmsgs")]
    public long TransmittedMessages { get; set; }

    /// <summary>
    /// Total number of bytes transmitted for messages.
    /// </summary>
    [JsonPropertyName("txbytes")]
    public long TransmittedBytes { get; set; }

    /// <summary>
    /// Total number of messages consumed, not including ignored messages (due to offset, etc).
    /// </summary>
    [JsonPropertyName("rxmsgs")]
    public long ReceivedMessages { get; set; }

    /// <summary>
    /// Total number of bytes received for messages.
    /// </summary>
    [JsonPropertyName("rxbytes")]
    public long ReceivedBytes { get; set; }

    /// <summary>
    /// Total number of messages received (consumer, same as ReceivedMessages), or total number of messages produced (possibly not yet transmitted) (producer).
    /// </summary>
    [JsonPropertyName("msgs")]
    public long Messages { get; set; }

    /// <summary>
    /// Dropped outdated messages.
    /// </summary>
    [JsonPropertyName("rx_ver_drops")]
    public long DroppedMessages { get; set; }

    /// <summary>
    /// Current number of messages in-flight to/from broker.
    /// </summary>
    [JsonPropertyName("msgs_inflight")]
    public long MessagesInFlight { get; set; }

    /// <summary>
    /// Next expected acked sequence (idempotent producer).
    /// </summary>
    [JsonPropertyName("next_ack_seq")]
    public long NextAckedSequence { get; set; } = -1;

    /// <summary>
    /// Next expected errored sequence (idempotent producer).
    /// </summary>
    [JsonPropertyName("next_err_seq")]
    public long NextErroredSequence { get; set; } = -1;

    /// <summary>
    /// Last acked internal message id (idempotent producer).
    /// </summary>
    [JsonPropertyName("acked_msgid")]
    public long AckedMessageId { get; set; } = -1;
}
