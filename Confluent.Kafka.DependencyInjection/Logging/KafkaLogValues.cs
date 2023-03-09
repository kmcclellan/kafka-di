namespace Confluent.Kafka.DependencyInjection.Logging;

using System.Collections;
using System.Text;

readonly struct KafkaLogValues : IReadOnlyList<KeyValuePair<string, object?>>
{
    readonly string message;
    readonly string client;
    readonly TopicPartitionOffset[]? offsets;

    public KafkaLogValues(string message, string client, TopicPartitionOffset[]? offsets)
    {
        this.message = message;
        this.client = client;
        this.offsets = offsets;
    }

    public int Count => this.offsets == null ? 1 : 2;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            return (index) switch
            {
                0 => new("KafkaClient", this.client),
                1 => new("KafkaOffsets", this.offsets),
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
            builder.AppendLine();

            foreach (var tpo in this.offsets)
            {
                builder.Append(' ', 2);
                builder.AppendLine(tpo.ToString());
            }
        }

        return builder.ToString();
    }
}
