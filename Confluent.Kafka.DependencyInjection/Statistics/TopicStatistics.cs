namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Statistics about a Kafka topic.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#topics">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class TopicStatistics
{
    /// <summary>
    /// Topic name.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    /// <summary>
    /// Age of client's topic object (milliseconds).
    /// </summary>
    [JsonPropertyName("age")]
    public long Age { get; set; }

    /// <summary>
    /// Age of metadata from broker for this topic (milliseconds).
    /// </summary>
    [JsonPropertyName("metadata_age")]
    public long MetadataAge { get; set; }

    /// <summary>
    /// Batch sizes in bytes.
    /// </summary>
    [JsonPropertyName("batchsize")]
    public WindowStatistics? BatchSize { get; set; }

    /// <summary>
    /// Batch message counts. See Window stats·
    /// </summary>
    [JsonPropertyName("batchcnt")]
    public WindowStatistics? BatchCount { get; set; }

    /// <summary>
    /// Partitions dict, key is partition id.
    /// </summary>
    [JsonPropertyName("partitions")]
    public IReadOnlyDictionary<string, PartitionStatistics>? Partitions { get; set; }

}
