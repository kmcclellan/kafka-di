using Microsoft.Extensions.Logging;

namespace Confluent.Kafka.DependencyInjection.Logging
{
    class LogEvents
    {
        public static EventId PartitionsAssigned = new EventId(10, nameof(PartitionsAssigned)),
            PartitionsRevoked = new EventId(11, nameof(PartitionsRevoked)),
            OffsetsCommitted = new EventId(20, nameof(OffsetsCommitted));
    }
}
