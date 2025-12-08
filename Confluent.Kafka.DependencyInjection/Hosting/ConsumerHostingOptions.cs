namespace Confluent.Kafka.Hosting
{
    using System.Collections.Generic;

    /// <summary>
    /// Options for consuming from Kafka in a hosted setting.
    /// </summary>
    public class ConsumerHostingOptions
    {
        /// <summary>
        /// Gets or sets whether to disable hosted consuming.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets a list of topic names from which to consume (using broker assignment).
        /// </summary>
        public IList<string> Subscription { get; } = new List<string>();

        /// <summary>
        /// Gets a list of manual partitions/offsets from which to consume.
        /// </summary>
        public IList<TopicPartitionOffset> Assignment { get; } = new List<TopicPartitionOffset>();

        /// <summary>
        /// Gets or sets the maximum number of Kafka messages the host may process concurrently.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the host should manually store offsets for processed messages.
        /// </summary>
        /// <remarks>
        /// Client configuration property <c>enable.auto.offset.store</c> must be set to <c>false</c>.
        /// </remarks>
        public bool StoreProcessedOffsets { get; set; }
    }
}
