namespace DistributedKit.Contracts;

/// <summary>
/// Serializable message context
/// </summary>
/// <param name="AccountId"></param>
/// <param name="MessageId"></param>
/// <param name="Message"></param>
[Serializable]
[GenerateSerializer]
[Alias("Rant.DistributedKit.Contracts.MessageContext`1")]
public record MessageContext<TMessage>(
    [property: Id(0)] Guid AccountId,
    [property: Id(1)] string MessageId,
    [property: Id(2)] TMessage Message
    )
{
    [Id(3)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
