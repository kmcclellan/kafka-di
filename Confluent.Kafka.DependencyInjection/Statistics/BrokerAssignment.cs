namespace Confluent.Kafka.DependencyInjection.Statistics;

using System.Text.Json.Serialization;

/// <summary>
/// Topic partition assigned to broker.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/confluentinc/librdkafka/blob/master/STATISTICS.md#brokerstoppars">librdkafka documentation</see> for more information.
/// </remarks>
public sealed class BrokerAssignment
{
    /// <summary>
    /// Topic name.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    /// <summary>
    /// Partition id.
    /// </summary>
    [JsonPropertyName("partition")]
    public long Partition { get; set; } = -1;
}
