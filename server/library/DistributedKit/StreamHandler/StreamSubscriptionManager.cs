using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DistributedKit.StreamHandler;

/// <summary>
/// Manages stream subscriptions for stream IDs
/// </summary>
public interface IStreamSubscriptionManager
{
    /// <summary>
    /// Subscribes to all available streams for the specified stream ID
    /// </summary>
    /// <param name="streamId">ID to subscribe for</param>
    Task SubscribeAllStreams(Guid streamId);

    /// <summary>
    /// Unsubscribes from all streams for the specified stream ID
    /// </summary>
    /// <param name="streamId">ID to unsubscribe</param>
    Task UnsubscribeAllStreams(Guid streamId);
}

internal sealed class StreamSubscriptionManager(ILogger<StreamSubscriptionManager> logger
    , IServiceProvider serviceProvider
    ) : IStreamSubscriptionManager
{
    private readonly ConcurrentDictionary<Guid, List<IPersistentStreamHandlerRunner>> _subscriptions = new();

    /// <inheritdoc />
    public async Task SubscribeAllStreams(Guid streamId)
    {
        // todo: Implement a grain in the Orleans cluster that will store session information and add logic to disconnect already active connections

        logger.LogDebug("Creating stream subscriptions for stream ID {StreamId}", streamId);

        // Get all handlers through the base interface
        var persistentStreamHandlerRunners = serviceProvider.GetServices<IPersistentStreamHandlerRunner>();
        var streamSubscriptions = new List<IPersistentStreamHandlerRunner>();

        foreach (var persistentStreamHandlerRunner in persistentStreamHandlerRunners)
        {
            try
            {
                logger.LogTrace("Start stream handler runner: {StreamId} : {MessageType}", streamId, persistentStreamHandlerRunner.GetMessageTypeName());
                await persistentStreamHandlerRunner.Start(streamId);
                streamSubscriptions.Add(persistentStreamHandlerRunner);
                logger.LogTrace("Finish stream handler runner: {StreamId} : {MessageType}", streamId, persistentStreamHandlerRunner.GetMessageTypeName());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception stream handler runner: {StreamId} : {MessageType}", streamId, persistentStreamHandlerRunner.GetMessageTypeName());
            }
        }

        // Save subscriptions
        _subscriptions.AddOrUpdate(
            streamId,
            streamSubscriptions,
            (_, _) => streamSubscriptions
        );

        logger.LogInformation("Created all stream subscriptions for streamId: {StreamId}, subscription count: {SubscriptionCount}", streamSubscriptions.Count, streamId);
    }

    /// <inheritdoc />
    public async Task UnsubscribeAllStreams(Guid streamId)
    {
        logger.LogDebug("Unsubscribing stream ID {StreamId} from all streams", streamId);

        if (_subscriptions.TryRemove(streamId, out var subscriptions))
        {
            foreach (var handle in subscriptions)
            {
                await handle.Stop();
            }

            logger.LogInformation("Unsubscribed stream ID {StreamId} from {Count} streams",
                streamId, subscriptions.Count);
        }
        else
        {
            logger.LogDebug("No subscriptions found for stream ID {StreamId}", streamId);
        }
    }
}
