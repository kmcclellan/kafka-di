using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Confluent.Kafka.DependencyInjection.Logging
{
    class OffsetLogValues : IReadOnlyList<KeyValuePair<string, object>>
    {
        public IClient Client { get; }
        public IEnumerable<TopicPartitionOffset> Offsets { get; }

        readonly string message;

        string? formatted;
        List<KeyValuePair<string, object>>? values;

        public OffsetLogValues(IClient client, IEnumerable<TopicPartitionOffset> offsets, string message)
        {
            Client = client;
            Offsets = offsets;
            this.message = message;
        }

        public override string ToString()
        {
            if (formatted == null)
            {
                var builder = new StringBuilder();
                builder.Append('[')
                    .Append(Client.Name)
                    .Append("] ")
                    .Append(message)
                    .AppendLine(":");

                foreach (var x in Offsets) builder.AppendLine(x.ToString());
                formatted = builder.ToString();
            }

            return formatted;
        }

        public KeyValuePair<string, object> this[int index] => Values[index];
        public int Count => Values.Count;
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();

        List<KeyValuePair<string, object>> Values
        {
            get
            {
                if (values == null)
                {
                    var topics = new HashSet<string>();
                    var partitions = new Dictionary<string, List<int>>();
                    var offsets = new Dictionary<string, List<long>>();

                    foreach (var tpo in Offsets)
                    {
                        if (topics.Add(tpo.Topic))
                        {
                            partitions.Add(tpo.Topic, new List<int> { tpo.Partition });
                            offsets.Add(tpo.Topic, new List<long> { tpo.Offset });
                        }
                        else
                        {
                            partitions[tpo.Topic].Add(tpo.Partition);
                            offsets[tpo.Topic].Add(tpo.Offset);
                        }
                    }

                    values = new Dictionary<string, object>
                    {
                        { "KafkaClient", Client.Name },
                        { "KafkaTopics", topics },
                        { "KafkaPartitions", partitions },
                        { "KafkaOffsets", offsets }
                    }.ToList();
                }

                return values;
            }
        }
    }
}
