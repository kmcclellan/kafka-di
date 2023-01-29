using System.Collections.Generic;

namespace Confluent.Kafka.DependencyInjection.Builders
{
    sealed class ConfigWrapper<TReceiver> : ConfigWrapper
    {
        public ConfigWrapper(IEnumerable<KeyValuePair<string, string>> values) : base(values) { }
    }

    class ConfigWrapper
    {
        public IEnumerable<KeyValuePair<string, string>> Values { get; }

        public ConfigWrapper(IEnumerable<KeyValuePair<string, string>> values)
        {
            Values = values;
        }
    }
}
