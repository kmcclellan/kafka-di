namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics about a Kafka broker.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#brokers">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class BrokerStatistics
{
    /// <summary>
    /// Broker hostname, port and broker id.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Broker id (-1 for bootstraps).
    /// </summary>
    [JsonPropertyName("nodeid")]
    public long NodeId { get; set; } = -1;

    /// <summary>
    /// Broker hostname.
    /// </summary>
    [JsonPropertyName("nodename")]
    public string? NodeName { get; set; }

    /// <summary>
    /// Broker source (learned, configured, internal, logical).
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Broker state (INIT, DOWN, CONNECT, AUTH, APIVERSION_QUERY, AUTH_HANDSHAKE, UP, UPDATE).
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Time since last broker state change (microseconds).
    /// </summary>
    [JsonPropertyName("stateage")]
    public long StateAge { get; set; }

    /// <summary>
    /// Number of requests awaiting transmission to broker.
    /// </summary>
    [JsonPropertyName("outbuf_cnt")]
    public long OutBufferRequests { get; set; }

    /// <summary>
    /// Number of messages awaiting transmission to broker.
    /// </summary>
    [JsonPropertyName("outbuf_msg_cnt")]
    public long OutBufferMessages { get; set; }

    /// <summary>
    /// Number of requests in-flight to broker awaiting response.
    /// </summary>
    [JsonPropertyName("waitresp_cnt")]
    public long WaitingResponses { get; set; }

    /// <summary>
    /// Number of messages in-flight to broker awaiting response.
    /// </summary>
    [JsonPropertyName("waitresp_msg_cnt")]
    public long WaitingMessages { get; set; }

    /// <summary>
    /// Total number of requests sent.
    /// </summary>
    [JsonPropertyName("tx")]
    public long TransmittedRequests { get; set; }

    /// <summary>
    /// Total number of bytes sent.
    /// </summary>
    [JsonPropertyName("txbytes")]
    public long TransmittedBytes { get; set; }

    /// <summary>
    /// Total number of transmission errors.
    /// </summary>
    [JsonPropertyName("txerrs")]
    public long TransmitErrors { get; set; }

    /// <summary>
    /// Total number of request retries.
    /// </summary>
    [JsonPropertyName("txretries")]
    public long TransmitRetries { get; set; }

    /// <summary>
    /// Microseconds since last socket send (or -1 if no sends yet for current connection).
    /// </summary>
    [JsonPropertyName("txidle")]
    public long TransmitIdleTime { get; set; } = -1;

    /// <summary>
    /// Total number of requests timed out.
    /// </summary>
    [JsonPropertyName("req_timeouts")]
    public long RequestTimeouts { get; set; }

    /// <summary>
    /// Total number of responses received.
    /// </summary>
    [JsonPropertyName("rx")]
    public long ReceivedResponses { get; set; }

    /// <summary>
    /// Total number of bytes received.
    /// </summary>
    [JsonPropertyName("rxbytes")]
    public long ReceivedBytes { get; set; }

    /// <summary>
    /// Total number of receive errors.
    /// </summary>
    [JsonPropertyName("rxerrs")]
    public long ReceiveErrors { get; set; }

    /// <summary>
    /// Total number of unmatched correlation ids in response (typically for timed out requests).
    /// </summary>
    [JsonPropertyName("rxcorriderrs")]
    public long ReceiveCorrelationErrors { get; set; }

    /// <summary>
    /// Total number of partial MessageSets received. The broker may return partial responses if the full MessageSet could not fit in the remaining Fetch response size.
    /// </summary>
    [JsonPropertyName("rxpartial")]
    public long ReceivedPartials { get; set; }

    /// <summary>
    /// Microseconds since last socket receive (or -1 if no receives yet for current connection).
    /// </summary>
    [JsonPropertyName("rxidle")]
    public long ReceiveIdleTime { get; set; } = -1;

    /// <summary>
    /// Request type counters. Key is the request name, value is the number of requests sent.
    /// </summary>
    [JsonPropertyName("req")]
    public IReadOnlyDictionary<string, long>? RequestTypes { get; set; }

    /// <summary>
    /// Total number of decompression buffer size increases.
    /// </summary>
    [JsonPropertyName("zbuf_grow")]
    public long DecompressionBufferGrow { get; set; }

    /// <summary>
    /// Total number of buffer size increases (deprecated, unused).
    /// </summary>
    [JsonPropertyName("buf_grow")]
    public long BufferGrow { get; set; }

    /// <summary>
    /// Broker thread poll loop wakeups.
    /// </summary>
    [JsonPropertyName("wakeups")]
    public long Wakeups { get; set; }

    /// <summary>
    /// Number of connection attempts, including successful and failed, and name resolution failures.
    /// </summary>
    [JsonPropertyName("connects")]
    public long Connects { get; set; }

    /// <summary>
    /// Number of disconnects (triggered by broker, network, load-balancer, etc.).
    /// </summary>
    [JsonPropertyName("disconnects")]
    public long Disconnects { get; set; }

    /// <summary>
    /// Internal producer queue latency in microseconds.
    /// </summary>
    [JsonPropertyName("int_latency")]
    public WindowStatistics? InternalLatency { get; set; }

    /// <summary>
    /// Internal request queue latency in microseconds. This is the time between a request is enqueued on the transmit (outbuf) queue and the time the request is written to the TCP socket. Additional buffering and latency may be incurred by the TCP stack and network.
    /// </summary>
    [JsonPropertyName("outbuf_latency")]
    public WindowStatistics? OutBufferLatency { get; set; }

    /// <summary>
    /// Broker latency / round-trip time in microseconds.
    /// </summary>
    [JsonPropertyName("rtt")]
    public WindowStatistics? RoundTripTime { get; set; }

    /// <summary>
    /// Broker throttling time in milliseconds.
    /// </summary>
    [JsonPropertyName("throttle")]
    public WindowStatistics? Throttle { get; set; }

    /// <summary>
    /// Partitions handled by this broker handle. Key is "topic-partition".
    /// </summary>
    [JsonPropertyName("toppars")]
    public IReadOnlyDictionary<string, BrokerAssignment>? TopicPartitions { get; set; }
}
