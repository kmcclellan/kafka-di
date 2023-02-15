namespace Confluent.Kafka.DependencyInjection.Clients;

using System.Collections.Generic;

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
