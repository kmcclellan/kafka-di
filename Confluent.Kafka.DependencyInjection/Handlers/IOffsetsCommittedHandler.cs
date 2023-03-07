namespace Confluent.Kafka.DependencyInjection.Handlers;

/// <summary>
/// A DI-friendly contract for handling automatic Kafka commits.
/// </summary>
/// <seealso cref="ConsumerBuilder{TKey, TValue}.SetOffsetsCommittedHandler(Action{IConsumer{TKey, TValue}, CommittedOffsets})"/>
public interface IOffsetsCommittedHandler
{
    /// <summary>
    /// Handles a Kafka auto-commit.
    /// </summary>
    /// <param name="consumer">The Kafka consumer.</param>
    /// <param name="offsets">The committed offsets.</param>
    void OnOffsetsCommitted(IClient consumer, CommittedOffsets offsets);
}
