namespace Confluent.Kafka.DependencyInjection.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

readonly struct KafkaLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private const string DefaultName = "Unknown";

    readonly IClient client;
    readonly object? payload;

    public KafkaLogState(IClient client, object? payload)
    {
        this.client = client;
        this.payload = payload;
    }

    public int Count => 2;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            return index switch
            {
                0 => new("KafkaClient", GetClientName()),
                1 => new("{OriginalMessage}", payload),
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
        return string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", GetClientName(), payload);
    }

    private string GetClientName() => client.Handle is null || client.Handle.IsInvalid
        ? DefaultName
        : client.Name;
}
