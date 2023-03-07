namespace Confluent.Kafka.DependencyInjection.Logging;

using System.Collections;
using System.Text;

readonly struct KafkaLogValues : IReadOnlyList<KeyValuePair<string, object?>>
{
    readonly string client;
    readonly string message;
    readonly IReadOnlyList<TopicPartitionOffset>? offsets;

    public KafkaLogValues(string client, string message, IReadOnlyList<TopicPartitionOffset>? offsets = null)
    {
        this.client = client;
        this.message = message;
        this.offsets = offsets;
    }

    public int Count => offsets == null ? 2 : 4;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            return (offsets == null ? 4 * index : index) switch
            {
                0 => new("KafkaClient", client),
                1 => new("KafkaTopics", GetOffsetValues(x => x.Topic)),
                2 => new("KafkaPartitions", GetOffsetValues(x => x.Partition.Value)),
                3 => new("KafkaOffsets", GetOffsetValues(x => x.Offset.Value)),
                4 => new("{OriginalMessage}", message),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int i = 0; i < Count;)
        {
            yield return this[i++];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var builder = new StringBuilder()
            .Append('[')
            .Append(client)
            .Append(']')
            .Append(' ')
            .Append(message);

        if (offsets != null)
        {
            builder.AppendLine(":");

            foreach (var tpo in offsets)
            {
                builder.AppendLine(tpo.ToString());
            }
        }

        return builder.ToString();
    }

    T[]? GetOffsetValues<T>(Func<TopicPartitionOffset, T> func)
    {
        if (offsets != null)
        {
            var values = new T[offsets.Count];

            for (var i = 0; i < offsets.Count;)
            {
                values[i++] = func(offsets[i]);
            }
        }

        return null;
    }
}
