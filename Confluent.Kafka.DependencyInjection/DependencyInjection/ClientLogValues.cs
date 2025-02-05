namespace Confluent.Kafka.DependencyInjection;

using System.Collections;
using System.Text;

readonly struct ClientLogValues(
    string message,
    string client,
    IEnumerable<TopicPartitionOffset>? offsets) :
    IReadOnlyList<KeyValuePair<string, object?>>
{
    public int Count => offsets == null ? 1 : 2;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            return index switch
            {
                0 => new("KafkaClient", client),
                1 => new("KafkaOffsets", offsets),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (var i = 0; i < this.Count;)
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
            .Append(client)
            .Append(']')
            .Append(' ')
            .Append(message);

        if (offsets != null)
        {
            builder.AppendLine();

            foreach (var tpo in offsets)
            {
                builder.Append(' ', 2);
                builder.AppendLine(tpo.ToString());
            }
        }

        return builder.ToString();
    }
}
