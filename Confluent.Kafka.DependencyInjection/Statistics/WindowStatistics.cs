namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Rolling window statistics. The values are in microseconds unless otherwise stated.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#window-stats">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class WindowStatistics
{
    /// <summary>
    /// Smallest value.
    /// </summary>
    [JsonPropertyName("min")]
    public long Minimum { get; set; }

    /// <summary>
    /// Largest value.
    /// </summary>
    [JsonPropertyName("max")]
    public long Maximum { get; set; }

    /// <summary>
    /// Average value.
    /// </summary>
    [JsonPropertyName("avg")]
    public long Average { get; set; }

    /// <summary>
    /// Sum of values.
    /// </summary>
    [JsonPropertyName("sum")]
    public long Sum { get; set; }

    /// <summary>
    /// Number of values sampled.
    /// </summary>
    [JsonPropertyName("cnt")]
    public long Count { get; set; }

    /// <summary>
    /// Standard deviation (based on histogram).
    /// </summary>
    [JsonPropertyName("stddev")]
    public long StandardDeviation { get; set; }

    /// <summary>
    /// Memory size of Hdr Histogram.
    /// </summary>
    [JsonPropertyName("hdrsize")]
    public long HdrSize { get; set; }

    /// <summary>
    /// 50th percentile.
    /// </summary>
    [JsonPropertyName("p50")]
    public long P50 { get; set; }

    /// <summary>
    /// 75th percentile.
    /// </summary>
    [JsonPropertyName("p75")]
    public long P75 { get; set; }

    /// <summary>
    /// 90th percentile.
    /// </summary>
    [JsonPropertyName("p90")]
    public long P90 { get; set; }

    /// <summary>
    /// 95th percentile.
    /// </summary>
    [JsonPropertyName("p95")]
    public long P95 { get; set; }

    /// <summary>
    /// 99th percentile.
    /// </summary>
    [JsonPropertyName("p99")]
    public long P99 { get; set; }

    /// <summary>
    /// 99.99th percentile.
    /// </summary>
    [JsonPropertyName("p99_99")]
    public long P9999 { get; set; }

    /// <summary>
    /// Values skipped due to out of histogram range.
    /// </summary>
    [JsonPropertyName("outofrange")]
    public long OutOfRange { get; set; }
}
