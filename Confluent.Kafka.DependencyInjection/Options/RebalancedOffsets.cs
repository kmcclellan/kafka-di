namespace Confluent.Kafka.Options;

/// <summary>
/// The state of partitions/offsets during a consumer group rebalance.
/// </summary>
public readonly struct RebalancedOffsets : IEquatable<RebalancedOffsets>
{
    /// <summary>
    /// Initializes the rebalanced offsets.
    /// </summary>
    /// <param name="offsets">The affected offsets</param>
    /// <param name="revoked">Whether the partitions are being revoked.</param>
    /// <param name="lost">Wehther the partitions have been lost.</param>
    public RebalancedOffsets(IReadOnlyList<TopicPartitionOffset> offsets, bool revoked = false, bool lost = false)
    {
        this.Offsets = offsets;
        this.Revoked = revoked;
        this.Lost = lost;
    }

    /// <summary>
    /// Gets the affected topics, partitions, and offsets.
    /// </summary>
    public IReadOnlyList<TopicPartitionOffset> Offsets { get; }

    /// <summary>
    /// Gets whether the partitions are being revoked.
    /// </summary>
    public bool Revoked { get; }

    /// <summary>
    /// Gets whether the partitions have been lost.
    /// </summary>
    public bool Lost { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is RebalancedOffsets offsets && this.Equals(offsets);
    }

    /// <inheritdoc/>
    public bool Equals(RebalancedOffsets other)
    {
        return this.Offsets == other.Offsets &&
            this.Revoked == other.Revoked &&
            this.Lost == other.Lost;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -2105332669 *
            (-1521134295 + this.Offsets.GetHashCode()) *
            (-1521134295 + this.Revoked.GetHashCode()) *
            (-1521134295 + this.Lost.GetHashCode());
    }

    /// <inheritdoc/>
    public static bool operator ==(RebalancedOffsets left, RebalancedOffsets right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(RebalancedOffsets left, RebalancedOffsets right)
    {
        return !(left == right);
    }
}
