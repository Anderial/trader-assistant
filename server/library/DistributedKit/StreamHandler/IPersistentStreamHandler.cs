using DistributedKit.Contracts;

namespace DistributedKit.StreamHandler;

/// <summary>
/// Base marker interface for stream handlers
/// </summary>
public interface IPersistentStreamHandler
{
    // Marker interface for stream handlers
}

/// <summary>
/// Generic handler for stream messages of a specific type
/// </summary>
/// <typeparam name="TMessage">Type of message to be handled</typeparam>
public interface IPersistentStreamHandlerWithGuidKey<TMessage> : IPersistentStreamHandler
{
    /// <summary>
    /// Handles a message from the stream
    /// </summary>
    /// <param name="message">Message to be processed</param>
    Task Execute(MessageContext<TMessage> message);
}
