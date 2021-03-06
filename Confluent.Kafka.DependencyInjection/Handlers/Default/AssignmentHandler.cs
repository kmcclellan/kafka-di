﻿using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka.DependencyInjection.Logging;
using Microsoft.Extensions.Logging;

namespace Confluent.Kafka.DependencyInjection.Handlers.Default
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class AssignmentHandler : IPartitionsAssignedHandler, IPartitionsRevokedHandler
    {
        readonly ILogger<AssignmentHandler> logger;

        public AssignmentHandler(ILogger<AssignmentHandler> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<TopicPartitionOffset> OnPartitionsAssigned(
            IClient client,
            IEnumerable<TopicPartition> partitions)
        {
            var offsets = partitions.Select(
                p => new TopicPartitionOffset(p, Offset.Unset));

            logger.LogKafkaAssignment(client, offsets);
            return offsets;
        }

        public IEnumerable<TopicPartitionOffset> OnPartitionsRevoked(
            IClient client,
            IEnumerable<TopicPartitionOffset> offsets)
        {
            logger.LogKafkaRevocation(client, offsets);
            return Enumerable.Empty<TopicPartitionOffset>();
        }
    }
}
