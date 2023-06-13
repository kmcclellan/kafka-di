namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics about a Kafka consumer group.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#cgrp">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class ConsumerGroupStatistics
{
    /// <summary>
    /// Local consumer group handler's state.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Time elapsed since last state change (milliseconds).
    /// </summary>
    [JsonPropertyName("stateage")]
    public long StateAge { get; set; }

    /// <summary>
    /// Local consumer group handler's join state.
    /// </summary>
    [JsonPropertyName("join_state")]
    public string? JoinState { get; set; }

    /// <summary>
    /// Time elapsed since last rebalance (assign or revoke) (milliseconds).
    /// </summary>
    [JsonPropertyName("rebalance_age")]
    public long RebalanceAge { get; set; }

    /// <summary>
    /// Total number of rebalances (assign or revoke).
    /// </summary>
    [JsonPropertyName("rebalance_cnt")]
    public long RebalanceCount { get; set; }

    /// <summary>
    /// Last rebalance reason, or empty string.
    /// </summary>
    [JsonPropertyName("rebalance_reason")]
    public string? RebalanceReason { get; set; }

    /// <summary>
    /// Current assignment's partition count.
    /// </summary>
    [JsonPropertyName("assignment_size")]
    public long AssignmentSize { get; set; }
}
