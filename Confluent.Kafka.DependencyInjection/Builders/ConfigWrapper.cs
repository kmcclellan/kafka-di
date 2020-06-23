using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Builders
{
    class ConfigWrapper
    {
        public IEnumerable<KeyValuePair<string, string>> Values { get; }

        public ConfigWrapper(IEnumerable<KeyValuePair<string, string>> values)
        {
            Values = values;
        }
    }
}
