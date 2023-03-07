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

    public int Count => this.offsets == null ? 2 : 4;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            return (this.offsets == null ? 4 * index : index) switch
            {
                0 => new("KafkaClient", this.client),
                1 => new("KafkaTopics", this.GetOffsetValues(x => x.Topic)),
                2 => new("KafkaPartitions", this.GetOffsetValues(x => x.Partition.Value)),
                3 => new("KafkaOffsets", this.GetOffsetValues(x => x.Offset.Value)),
                4 => new("{OriginalMessage}", this.message),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int i = 0; i < this.Count;)
        {
            yield return this[i++];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public override string ToString()
    {
        var builder = new StringBuilder()
            .Append('[')
            .Append(this.client)
            .Append(']')
            .Append(' ')
            .Append(this.message);

        if (this.offsets != null)
        {
            builder.AppendLine(":");

            foreach (var tpo in this.offsets)
            {
                builder.AppendLine(tpo.ToString());
            }
        }

        return builder.ToString();
    }

    T[]? GetOffsetValues<T>(Func<TopicPartitionOffset, T> func)
    {
        if (this.offsets != null)
        {
            var values = new T[this.offsets.Count];

            for (var i = 0; i < this.offsets.Count;)
            {
                values[i++] = func(this.offsets[i]);
            }
        }

        return null;
    }
}
