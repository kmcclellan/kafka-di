using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Confluent.Kafka.DependencyInjection.Logging
{
    class LogEvents
    {
        static Dictionary<ErrorCode, EventId>? errorIds;

        public static EventId PartitionsAssigned = new(10, nameof(PartitionsAssigned)),
            PartitionsRevoked = new(11, nameof(PartitionsRevoked)),
            OffsetsCommitted = new(20, nameof(OffsetsCommitted));

        public static EventId FromError(ErrorCode code)
        {
            errorIds ??= Enum.GetValues(typeof(ErrorCode))
                .Cast<ErrorCode>()
                .ToDictionary(x => x, x => new EventId((int)x, x.ToString()));

            return errorIds[code];
        }
    }
}
